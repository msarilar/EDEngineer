using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Utils;
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
        private readonly ServerBridge serverBridge;

        public MainWindow()
        {
            var procName = Process.GetCurrentProcess().ProcessName;
            var processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {
                System.Windows.Forms.MessageBox.Show($"EDEngineer already running, you can bring it up with your shortcut ({SettingsManager.Shortcut}).",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Current.Shutdown();
                return;
            }
            SettingsManager.Init();

            try
            {
                ReleaseNotesManager.ShowReleaseNotes();
            }
            catch
            {
                // silently fail if release notes can't be shown
            }

            Languages.InitLanguages();

            InitializeComponent();

            if (Properties.Settings.Default.WindowUnlocked)
            {
                AllowsTransparency = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
                Topmost = false;
                ShowInTaskbar = true;
            }
            else
            {
                AllowsTransparency = true;
                WindowStyle = WindowStyle.None;
                Topmost = true;
                ShowInTaskbar = false;
            }

            viewModel = new MainWindowViewModel(Languages.Instance);
            DataContext = viewModel;

            RefreshCargoSources();
            viewModel.PropertyChanged += (o, e) =>
                                         {
                                             if (e.PropertyName == "ShowOnlyForFavorites" ||
                                                 e.PropertyName == "ShowZeroes" ||
                                                 e.PropertyName == "CurrentCommander")
                                             {
                                                 RefreshCargoSources();
                                             }
                                         };
            serverBridge = new ServerBridge(viewModel, SettingsManager.AutoRunServer);
        }

        public void RefreshCargoSources()
        {
            var commander = viewModel.CurrentCommander.Value;

            var blueprintSource = new CollectionViewSource { Source = commander.State.Blueprints };
            commander.Filters.Monitor(blueprintSource, commander.State.Cargo.Select(c => c.Value));
            Blueprints.ItemsSource = blueprintSource.View;

            Commodities.ItemsSource = commander.FilterView(viewModel, Kind.Commodity, new CollectionViewSource { Source = commander.State.Cargo });
            Materials.ItemsSource = commander.FilterView(viewModel, Kind.Material, new CollectionViewSource { Source = commander.State.Cargo });
            Data.ItemsSource = commander.FilterView(viewModel, Kind.Data, new CollectionViewSource { Source = commander.State.Cargo });
        }

        private int ToolbarHeight => SystemInformation.CaptionHeight + 6; // couldn't find a proper property returning "29" which is the height I need

        private void MainWindowLoaded(object sender, RoutedEventArgs args)
        {
            var dimensions = SettingsManager.Dimensions;

            Width = dimensions.Width;
            Left = dimensions.Left;
            Top = dimensions.Top;
            Height = dimensions.Height;

            if (dimensions.LeftSideWidth != 1 || dimensions.RightSideWidth != 1)
            {
                ContentGrid.ColumnDefinitions[0].Width = new GridLength(dimensions.LeftSideWidth, GridUnitType.Star);
                ContentGrid.ColumnDefinitions[2].Width = new GridLength(dimensions.RightSideWidth, GridUnitType.Star);
            }

            if (AllowsTransparency)
            {
                ToggleEditMode.Content = viewModel.Languages.Translate("Unlock Window");
                Splitter.Visibility = Visibility.Hidden;
            }
            else
            {
                ToggleEditMode.Content = viewModel.Languages.Translate("Lock Window");
                ResetWindowPositionButton.Visibility = Visibility.Visible;
            }

            icon = TrayIconManager.Init((o, e) => ShowWindow(), 
                (o, e) => Close(), ConfigureShortcut, 
                (o, e) => ToggleEditModeChecked(o, null),
                (o, e) => ResetWindowPositionButtonClicked(o, null),
                (o, e) => Languages.PromptLanguage(viewModel.Languages),
                () => serverBridge.Toggle(),
                serverBridge.Running);

            var shortcut = SettingsManager.Shortcut;

            HotkeyManager.RegisterHotKey(this, (Keys)new KeysConverter().ConvertFromString(shortcut));
            Blueprints.UpdateLayout();
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
            if (ShortcutPrompt.ShowDialog(SettingsManager.Shortcut, out shortcut))
            {
                SettingsManager.Shortcut = shortcut;
                HotkeyManager.UnregisterHotKey(this);
                HotkeyManager.RegisterHotKey(this, (Keys) new KeysConverter().ConvertFromString(shortcut));
            }
            else
            {
                HotkeyManager.RegisterHotKey(this, (Keys)new KeysConverter().ConvertFromString(SettingsManager.Shortcut));
            }

            ignoreShortcut = false;
        }

        private readonly Regex forbiddenCharacters = new Regex("[^0-9.-]+");
        private void EntryCountTextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (forbiddenCharacters.IsMatch(e.Text))
            {
                e.Handled = true;
            }

        }

        private BindingBase binding;
        private void EntryCountTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var box = (System.Windows.Controls.TextBox)sender;
            binding = box.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).ParentBindingBase;
            BindingOperations.ClearBinding(box, System.Windows.Controls.TextBox.TextProperty);
            box.SelectAll();
        }

        private void EntryCountTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            var box = (System.Windows.Controls.TextBox)sender;

            int newCount;
            if (int.TryParse(box.Text, out newCount))
            {
                var entry = (Entry)box.Tag;
                viewModel.UserChange(entry, newCount - entry.Count);
            }

            BindingOperations.SetBinding(box, System.Windows.Controls.TextBox.TextProperty, binding);
        }

        private void IncrementButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = (Entry) ((Button) sender).Tag;
            viewModel.UserChange(entry, 1);
        }

        private void DecrementButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = (Entry) ((Button) sender).Tag;
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
                if (child is Run)
                {
                    return null;
                }

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
                Show();
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
            if (handle != null)
            {
                HotkeyManager.UnregisterHotKey(this);
                handle.RemoveHook(WndProc);
                icon.Dispose();
            }

            viewModel?.LogWatcher?.Dispose();
            serverBridge?.Dispose();
        }

        private void WindowActivatedCompleted(object sender, EventArgs e)
        {
            transitionning = false;
        }

        private void WindowDeactivatedCompleted(object sender, EventArgs e)
        {
            Hide();
            transitionning = false;
        }

        private void ToggleEditModeChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.WindowUnlocked = !Properties.Settings.Default.WindowUnlocked;
            Properties.Settings.Default.Save();

            var w = new MainWindow();
            Close();
            w.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!bypassPositionSave)
            {
                var coords = PointToScreen(new Point(0, 0));
                var modificator = AllowsTransparency ? 0 : ToolbarHeight;
                SettingsManager.Dimensions = new WindowDimensions()
                {
                    Height = ActualHeight,
                    Left = coords.X,
                    Top = coords.Y - modificator,
                    Width = ActualWidth,
                    LeftSideWidth = ContentGrid.ColumnDefinitions[0].Width.Value,
                    RightSideWidth = ContentGrid.ColumnDefinitions[2].Width.Value
                };
            }

            base.OnClosing(e);
        }

        private bool bypassPositionSave = false;

        private void ResetWindowPositionButtonClicked(object sender, RoutedEventArgs e)
        {
            SettingsManager.Dimensions.Reset();
            bypassPositionSave = true;

            Properties.Settings.Default.WindowUnlocked = false;
            Properties.Settings.Default.Save();

            var w = new MainWindow();
            Close();
            w.Show();
        }

        private void CheckAllButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeAllFilters(true);
        }

        private void UncheckAllButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeAllFilters(false);
        }
    }
}