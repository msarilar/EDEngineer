using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EDEngineer.Models;
using EDEngineer.Utils.System;
using EDEngineer.Utils.UI;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using DataGridCell = System.Windows.Controls.DataGridCell;

namespace EDEngineer
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            var procName = Process.GetCurrentProcess().ProcessName;
            // get the list of all processes by the "procName"       
            var processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {
                System.Windows.Forms.MessageBox.Show($"EDEngineer already running, you can bring it up with your shortcut ({Properties.Settings.Default.Shortcut}).",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Current.Shutdown();
                return;
            }

            if (IOManager.NewVersionAvailable)
            {
                if (System.Windows.Forms.MessageBox.Show($"New version available!{Environment.NewLine}Would you like to close the application and download it?", "Update available", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.OK)
                {
                    Process.Start("https://github.com/msarilar/EDEngineer/releases/latest");
                    Application.Current.Shutdown();
                    return;
                }
            }

            InitializeComponent();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            var blueprintsView = new CollectionViewSource {Source = viewModel.Blueprints}.View;
            viewModel.Filters.Monitor(blueprintsView, viewModel.State.Cargo.Select(c => c.Value));
            Blueprints.ItemsSource = blueprintsView;

            Commodities.ItemsSource = viewModel.FilterView(Kind.Commodity, new CollectionViewSource { Source = viewModel.State.Cargo }.View);
            Materials.ItemsSource = viewModel.FilterView(Kind.Material, new CollectionViewSource { Source = viewModel.State.Cargo }.View);
            Data.ItemsSource = viewModel.FilterView(Kind.Data, new CollectionViewSource { Source = viewModel.State.Cargo }.View);
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs args)
        {
            icon = TrayIconManager.Init((o, e) => ShowWindow(), (o, e) => Close(), ConfigureShortcut);

            var shortcut = Properties.Settings.Default.Shortcut;
            var converter = new KeysConverter();
            if (string.IsNullOrEmpty(shortcut))
            {
                shortcut = Properties.Settings.Default.Shortcut = converter.ConvertToString(Keys.F10 | Keys.Control);
                Properties.Settings.Default.Save();
            }

            HotkeyManager.RegisterHotKey(this, (Keys)new KeysConverter().ConvertFromString(shortcut));
        }

        private void ConfigureShortcut(object sender, EventArgs e)
        {
            string shortcut;
            ignoreShortcut = true;
            if (Math.Abs(Opacity) > 0.99)
            {
                HideWindow();
            }

            HotkeyManager.UnregisterHotKey(this);
            if (ShortcutPrompt.ShowDialog(Properties.Settings.Default.Shortcut, out shortcut))
            {
                Properties.Settings.Default.Shortcut = shortcut;
                Properties.Settings.Default.Save();
                HotkeyManager.UnregisterHotKey(this);
                HotkeyManager.RegisterHotKey(this, (Keys) new KeysConverter().ConvertFromString(shortcut));
            }
            else
            {
                HotkeyManager.RegisterHotKey(this, (Keys)new KeysConverter().ConvertFromString(Properties.Settings.Default.Shortcut));
            }

            ignoreShortcut = false;
        }

        private void CheckAllButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.Filters.ChangeAllFilters(true);
        }

        private void UncheckAllButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.Filters.ChangeAllFilters(false);
        }

        private void IncrementButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = ((KeyValuePair<string, Entry>) ((Button) sender).DataContext).Value;
            viewModel.UserChange(entry, 1);
        }

        private void DecrementButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = ((KeyValuePair<string, Entry>) ((Button) sender).DataContext).Value;
            viewModel.UserChange(entry, -1);
        }

        private void ChangeFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.LoadState(true);
        }

        private void PreviewMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            var cell = (DataGridCell) sender;
            if (cell.Column.Header == null)
            {
                var toggleButton = FindVisualParent<ToggleButton>(Mouse.DirectlyOver as DependencyObject);
                if (toggleButton?.IsChecked != null)
                {
                    toggleButton.IsChecked = !toggleButton.IsChecked.Value;
                }
                e.Handled = true;
            }
            else if (!cell.IsEditing)
            {
                var row = FindVisualParent<DataGridRow>(cell);
                if (row != null)
                {
                    row.IsSelected = !row.IsSelected;
                    e.Handled = true;
                }
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                var parentObject = VisualTreeHelper.GetParent(child);

                if (parentObject == null)
                {
                    return null;
                }

                var parent = parentObject as T;
                if (parent != null)
                {
                    return parent;
                }

                child = parentObject;
            }
        }

        private void BlueprintsDataGridLoaded(object sender, RoutedEventArgs e)
        {
            var newStyle = new Style
            {
                BasedOn = Blueprints.CellStyle,
                TargetType = typeof (DataGridCell)
            };

            newStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDownHandler)));
            Blueprints.CellStyle = newStyle;
        }

        bool transitionning = false;
        private void HideWindow()
        {
            if (!transitionning)
            {
                transitionning = true;
                var sb = (Storyboard)FindResource("WindowDeactivated");
                Storyboard.SetTarget(sb, this);
                sb.Begin();
            }
        }

        private void ShowWindow()
        {
            if (!transitionning)
            {
                Focus();
                transitionning = true;
                var sb = (Storyboard) FindResource("WindowActivated");
                Storyboard.SetTarget(sb, this);
                sb.Begin();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            handle = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            handle?.AddHook(WndProc);
        }
        
        private HwndSource handle;
        private IDisposable icon;
        private bool ignoreShortcut = false;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == HotkeyManager.WM_HOTKEY && !ignoreShortcut)
            {
                if (Math.Abs(Opacity) < 0.01)
                {
                    ShowWindow();
                }
                else
                {
                    HideWindow();
                }
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            HotkeyManager.UnregisterHotKey(this);
            handle.RemoveHook(WndProc);
            icon.Dispose();
        }

        private void TransitionCompleted(object sender, EventArgs e)
        {
            transitionning = false;
        }
    }
}