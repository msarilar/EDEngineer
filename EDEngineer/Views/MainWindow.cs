﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Filters;
using EDEngineer.Models.Utils.Collections;
using EDEngineer.Utils;
using EDEngineer.Utils.System;
using EDEngineer.Utils.UI;
using EDEngineer.Views.Popups;
using EDEngineer.Views.Popups.Graphics;
using Application = System.Windows.Application;
using WpfButton = System.Windows.Controls.Button;
using WinformContextMenu = System.Windows.Forms.ContextMenuStrip;
using DataGridCell = System.Windows.Controls.DataGridCell;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using NotificationSettingsWindow = EDEngineer.Views.Notifications.NotificationSettingsWindow;

namespace EDEngineer.Views
{
    public partial class MainWindow : System.Windows.Window
    {
        private MainWindowViewModel viewModel;
        private ServerBridge serverBridge;
        private PostponeScheduler saveDimensionScheduler;

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
            InitializeComponent();
            SettingsManager.Init();

            try
            {
                ReleaseNotesManager.ShowReleaseNotesIfNecessary();
                if (ReleaseNotesManager.RequireReset)
                {
                    Properties.Settings.Default.ClearAggregation = true;
                    Properties.Settings.Default.Save();
                }
            }
            catch
            {
                // silently fail if release notes can't be shown
            }

            NotificationSettingsWindow.InitNotifications();

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

            var dimensions = SettingsManager.Dimensions;

            Width = dimensions.Width;
            Left = dimensions.Left;
            Top = dimensions.Top;
            Height = dimensions.Height;
            WindowState = SettingsManager.Maximized ? WindowState.Maximized : WindowState;
 
            var logDirectory = Helpers.RetrieveLogDirectory(false, null);
            var task = Task.Factory.StartNew(() =>
            {
                viewModel = new MainWindowViewModel(Languages.Instance, logDirectory);
                if (ReleaseNotesManager.RequireReset)
                {
                    viewModel.ChangeAllFilters(true);
                }

                viewModel.PropertyChanged += (o, e) =>
                                             {
                                                 if (e.PropertyName == "ShowOnlyForFavorites" ||
                                                     e.PropertyName == "ShowZeroes" ||
                                                     e.PropertyName == "ShowFull" ||
                                                     e.PropertyName == "CurrentCommander" ||
                                                     e.PropertyName == "MaterialSubkindFilter" ||
                                                     e.PropertyName == "IngredientsGrouped")
                                                 {
                                                     RefreshCargoSources();
                                                 }

                                                 if (e.PropertyName == "IngredientsGrouped")
                                                 {
                                                     viewModel.CurrentComparer = viewModel.CurrentComparer;
                                                 }
                                             };
            });

            task.ContinueWith(t =>
                              {
                                  new Popups.ErrorWindow(t.Exception?.Flatten() ?? new Exception("Unknown Error")).ShowDialog();
                                  Close();
                              }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());
            
            task.ContinueWith(t =>
            {
                serverBridge = new ServerBridge(viewModel, SettingsManager.AutoRunServer);
                DataContext = viewModel;
                RefreshCargoSources();
                PostLoad(dimensions);
                if (!SettingsManager.SilentLaunch)
                {
                    var sb = (Storyboard)FindResource("HideSplash");
                    Storyboard.SetTarget(sb, Splash);
                    sb.Begin();
                }

                viewModel.InitiateWatch();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

            if (SettingsManager.SilentLaunch)
            {
                Splash.Visibility = Visibility.Collapsed;
                if (Properties.Settings.Default.WindowUnlocked)
                {
                    WindowState = WindowState.Minimized;
                }
                else
                {
                    Opacity = 0.001;
                }
            }
        }

        public void RefreshCargoSources()
        {
            var commander = viewModel.CurrentCommander.Value;

            var blueprintSource = new CollectionViewSource { Source = commander.State.Blueprints };
            commander.Filters.Monitor(blueprintSource, commander.State.Cargo.Ingredients.Select(c => c.Value), commander.HighlightedEntryData);
            Blueprints.ItemsSource = blueprintSource.View;
            
            Materials.ItemsSource = commander.FilterView(viewModel, Kind.Material, new CollectionViewSource { Source = commander.State.Cargo.Ingredients });
            Data.ItemsSource = commander.FilterView(viewModel, Kind.Data, new CollectionViewSource { Source = commander.State.Cargo.Ingredients });
            Commodities.ItemsSource = commander.FilterView(viewModel, Kind.Commodity, new CollectionViewSource { Source = commander.State.Cargo.Ingredients });
            Odyssey.ItemsSource = commander.FilterView(viewModel, Kind.OdysseyIngredient, new CollectionViewSource { Source = commander.State.Cargo.Ingredients });
        }

        private void PostLoad(WindowDimensions dimensions)
        {
            if (dimensions.LeftSideWidth != 1 || dimensions.RightSideWidth != 1)
            {
                ContentGrid.ColumnDefinitions[0].Width = new GridLength(dimensions.LeftSideWidth, GridUnitType.Star);
                ContentGrid.ColumnDefinitions[2].Width = new GridLength(dimensions.RightSideWidth, GridUnitType.Star);
            }

            if (AllowsTransparency)
            {
                ToggleEditMode.Content = viewModel.Languages.Translate("Unlock Window");
                MainSplitter.Visibility = Visibility.Hidden;
            }
            else
            {
                ToggleEditMode.Content = viewModel.Languages.Translate("Lock Window");
                ResetWindowPositionButton.Visibility = Visibility.Visible;
            }

            menu = TrayIconManager.BuildContextMenu((o, e) => ShowWindow(),
                (o, e) => Close(),
                ConfigureShortcut,
                (o, e) => ToggleEditModeChecked(o, null),
                (o, e) => ResetWindowPositionButtonClicked(o, null),
                (o, e) => Languages.PromptLanguage(viewModel.Languages),
                () => serverBridge.Toggle(),
                serverBridge.Running,
                (o, e) => { ReleaseNotesManager.ShowReleaseNotes(); },
                Properties.Settings.Default.CurrentVersion,
                (o, e) => { new NotificationSettingsWindow(viewModel.Languages).ShowDialog(); },
                (o, e) => { new GraphicSettingsWindow(viewModel.GraphicSettings).ShowDialog(); },
                (o, e) =>
                {
                    System.Diagnostics.Process.Start($"http://localhost:{SettingsManager.ServerPort}/{viewModel.CurrentCommander.Key}/chart");
                },
                (o, e) => ClearAggregationAndRestart(o, null),
                (o, e) => { new SettingsExportWindow(Restart, RefreshShoppingList).ShowDialog(); });

            icon = TrayIconManager.Init(menu);

            try
            {
                var shortcut = SettingsManager.Shortcut;
                var hotKey = (Keys) new KeysConverter().ConvertFromString(shortcut);

                HotkeyManager.RegisterHotKey(this, hotKey);
            }
            catch
            {
                SettingsManager.Shortcut = null;
                ConfigureShortcut(this, EventArgs.Empty);
                ShowWindow();
            }

            Blueprints.UpdateLayout();
            ShoppingList.UpdateLayout();

            if (!AllowsTransparency)
            {
                saveDimensionScheduler = new PostponeScheduler(SaveDimensions, 500);
                SizeChanged += (o, e) => saveDimensionScheduler.Schedule();
                LocationChanged += (o, e) => saveDimensionScheduler.Schedule();
                MainSplitter.DragCompleted += (o, e) => saveDimensionScheduler.Schedule();
            }
        }

        private void ConfigureShortcut(object sender, EventArgs e)
        {
            ignoreShortcut = true;
            if (Math.Abs(Opacity) > 0.99)
            {
                HideWindow();
            }

            HotkeyManager.UnregisterHotKey(this);
            if (ShortcutPrompt.ShowDialog(SettingsManager.Shortcut, out var shortcut))
            {
                SettingsManager.Shortcut = shortcut;
                HotkeyManager.UnregisterHotKey(this);
                HotkeyManager.RegisterHotKey(this, (Keys) new KeysConverter().ConvertFromString(shortcut));
            }
            else
            {
                HotkeyManager.RegisterHotKey(this, (Keys) new KeysConverter().ConvertFromString(SettingsManager.Shortcut));
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
        private void EntryCountTextBoxOnFocussed(object sender, RoutedEventArgs e)
        {
            var box = (System.Windows.Controls.TextBox)sender;
            binding = box.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).ParentBindingBase;
            BindingOperations.ClearBinding(box, System.Windows.Controls.TextBox.TextProperty);
            box.SelectAll();
        }

        private void EntryCountTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            var box = (System.Windows.Controls.TextBox)sender;

            if (int.TryParse(box.Text, out var newCount))
            {
                var entry = (Entry)box.Tag;
                viewModel.UserChange(entry, newCount - entry.Count);
            }

            BindingOperations.SetBinding(box, System.Windows.Controls.TextBox.TextProperty, binding);
        }

        private void IncrementButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = (Entry) ((WpfButton) sender).Tag;
            viewModel.UserChange(entry, 1);
        }

        private void DecrementButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = (Entry) ((WpfButton) sender).Tag;
            viewModel.UserChange(entry, -1);
        }

        private void DataGridOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var toggleButton = XamlHelpers.FindVisualParent<WpfButton>(Mouse.DirectlyOver as DependencyObject);
            if (toggleButton != null)
            {
                toggleButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                e.Handled = true;
            }
        }

        private void PreviewMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            var cell = (DataGridCell) sender;
            if (cell.Column.Header == null)
            {
                var toggleButton = XamlHelpers.FindVisualParent<ToggleButton>(Mouse.DirectlyOver as DependencyObject);
                if (toggleButton?.IsChecked != null)
                {
                    toggleButton.IsChecked = !toggleButton.IsChecked.Value;
                }
                e.Handled = true;
            }
            else if (!cell.IsEditing)
            {
                var row = XamlHelpers.FindVisualParent<DataGridRow>(cell);
                if (row != null)
                {
                    row.IsSelected = !row.IsSelected;
                    e.Handled = true;
                }
            }
        }

        private void DataGridLoaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = (System.Windows.Controls.DataGrid) sender;
            var newStyle = new Style
            {
                BasedOn = dataGrid.CellStyle,
                TargetType = typeof (DataGridCell)
            };

            newStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDownHandler)));
            dataGrid.CellStyle = newStyle;
        }

        private bool transitionning;
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
        private WinformContextMenu menu;
        private bool ignoreShortcut;

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
            if (!skipSaveAggregation)
            {
                viewModel?.SaveAggregation();
            }

            HotkeyManager.UnregisterHotKey(this);
            handle?.RemoveHook(WndProc);
            icon?.Dispose();
            viewModel?.Dispose();
            serverBridge?.Dispose();
            saveDimensionScheduler?.Dispose();
        }

        private bool skipSaveAggregation = false;
        private void ChangeFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            var newDirectory = Helpers.RetrieveLogDirectory(true, viewModel.LogDirectory);
            if (newDirectory != viewModel.LogDirectory)
            {
                skipSaveAggregation = true;
                viewModel.LogDirectory = newDirectory;
                Restart();
            }
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

            Restart();
        }

        private void ClearAggregationAndRestart(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ClearAggregation = true;
            Properties.Settings.Default.Save();

            Restart();
        }

        private void Restart()
        {
            var w = new MainWindow();
            Close();
            w.Show();
        }

        private void RefreshShoppingList(StringCollection shoppingList)
        {
            viewModel.CurrentCommander.Value.ClearShoppingList();
            viewModel.CurrentCommander.Value.RefreshShoppingList(shoppingList);
        }

        private void SaveDimensions()
        {
            SettingsManager.Dimensions = new WindowDimensions
            {
                Height = ActualHeight,
                Left = Left,
                Top = Top,
                Width = ActualWidth,
                LeftSideWidth = ContentGrid.ColumnDefinitions[0].Width.Value,
                RightSideWidth = ContentGrid.ColumnDefinitions[2].Width.Value
            };
            SettingsManager.Maximized = this.WindowState == WindowState.Maximized;
        }

        private void ResetWindowPositionButtonClicked(object sender, RoutedEventArgs e)
        {
            SettingsManager.Dimensions.Reset();

            Properties.Settings.Default.WindowUnlocked = false;
            Properties.Settings.Default.Save();

            Restart();
        }

        private void PrintShoppingListButtonClicked(object sender, RoutedEventArgs e)
        {
            XamlHelpers.SaveToPng(ShoppingList, Path.ChangeExtension(Path.GetTempFileName(), "png"));
        }

        private void ClearShoppingListButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentCommander.Value.ClearShoppingList();
            ShoppingListSplitterDoubleClicked(null, null);
        }

        private void ImportShoppingList(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentCommander.Value.ImportShoppingList();
        }

        private void ExportShoppingList(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentCommander.Value.ExportShoppingList();
        }

        private void CheckAllButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeAllFilters(true);
        }

        private void UncheckAllButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeAllFilters(false);
        }

        private void IngredientOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (KeyValuePair<string, Entry>) ((Grid) sender).DataContext;
            viewModel.ToggleHighlight(dataContext.Value);
        }

        private void IncrementShoppingList(object sender, RoutedEventArgs e)
        {
            var tag = ((WpfButton) sender).Tag;
            viewModel.CurrentCommander.Value.ShoppingListChange((Blueprint) tag, 1);
        }

        private void DecrementShoppingList(object sender, RoutedEventArgs e)
        {
            var tag = ((WpfButton) sender).Tag;
            viewModel.CurrentCommander.Value.ShoppingListChange((Blueprint)tag, -1);

            if (!viewModel.CurrentCommander.Value.ShoppingList.Composition.Any())
            {
                ShoppingListSplitterDoubleClicked(null, null);
            }
        }

        private void RemoveShoppingListBlock(object sender, RoutedEventArgs e)
        {
            var blueprints = (SortedObservableCollection<Tuple<Blueprint, int>>) ((WpfButton)sender).Tag;
            foreach (var blueprint in blueprints)
            {
                viewModel.CurrentCommander.Value.ShoppingListChange(blueprint.Item1, -1 * blueprint.Item1.ShoppingListCount);
            }

            if (!viewModel.CurrentCommander.Value.ShoppingList.Composition.Any())
            {
                ShoppingListSplitterDoubleClicked(null, null);
            }
        }

        private void ShoppingListSplitterDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            BlueprintsRow.Height = new GridLength(1, GridUnitType.Star);
            ShoppingListSplitterRow.Height = new GridLength(1, GridUnitType.Auto);
            ShoppingListRow.Height = new GridLength(1, GridUnitType.Auto);
        }

        private void ShoppingListBlueprintMouseEnter(object sender, MouseEventArgs e)
        {
            var ingredients = (List<BlueprintIngredient>) ((Grid) sender).Tag;
            var blueprint = (Tuple<Blueprint, int>)((Grid)sender).DataContext;
            viewModel.CurrentCommander.Value.HighlightShoppingListIngredient(ingredients, blueprint.Item1, true);
        }

        private void ShoppingListBlueprintMouseLeave(object sender, MouseEventArgs e)
        {
            var ingredients = (List<BlueprintIngredient>)((Grid)sender).Tag;
            var blueprint = (Tuple<Blueprint, int>)((Grid)sender).DataContext;
            viewModel.CurrentCommander.Value.HighlightShoppingListIngredient(ingredients, blueprint.Item1, false);
        }

        private void ShoppingListIngredientMouseEnter(object sender, MouseEventArgs e)
        {
            var ingredient = (BlueprintIngredient) ((TextBlock) sender).DataContext;
            var blueprints = (List<ShoppingListBlock>)((TextBlock)sender).Tag;
            viewModel.CurrentCommander.Value.HighlightShoppingListBlueprint(blueprints.SelectMany(b => b.Composition).ToList(), ingredient, true);
        }

        private void ShoppingListIngredientMouseLeave(object sender, MouseEventArgs e)
        {
            var ingredient = (BlueprintIngredient)((TextBlock)sender).DataContext;
            var blueprints = (List<ShoppingListBlock>)((TextBlock)sender).Tag;
            viewModel.CurrentCommander.Value.HighlightShoppingListBlueprint(blueprints.SelectMany(b => b.Composition).ToList(), ingredient, false);
        }

        private void SettingsButtonClicked(object sender, RoutedEventArgs e)
        {
            var p = PointToScreen(Mouse.GetPosition(this));
            menu.Show(new System.Drawing.Point((int)p.X, (int)p.Y), ToolStripDropDownDirection.BelowRight);
        }

        private void UnhighlightButtonClicked(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentCommander.Value.HighlightedEntryData.ToList().ForEach(entry => viewModel.ToggleHighlight(entry));
        }

        private void MaterialSubkindFilterChecked(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            if (ShowAllMaterialsRadioButton.IsChecked == true)
            {
                viewModel.MaterialSubkindFilter = null;
            }
            else if (ShowOnlyManufacturedMaterialsButton.IsChecked == true)
            {
                viewModel.MaterialSubkindFilter = Subkind.Manufactured;
            }
            else if (ShowOnlyRawMaterialsRadioButton.IsChecked == true)
            {
                viewModel.MaterialSubkindFilter = Subkind.Raw;
            }
        }

        private void CategoryFilterPanelClicked(object sender, MouseButtonEventArgs e)
        {
            var filter = ((CategoryFilter) ((StackPanel) sender).Tag);
            filter.Checked = !filter.Checked;
            e.Handled = true;
        }

        private void ShowAllGradesClicked(object sender, RoutedEventArgs e)
        {
            var button = ((ToggleButton) sender);
            viewModel.CurrentCommander.Value.ShowAllGradeChanges((ShoppingListBlock) button.Tag);
            e.Handled = true;
        }

        private void ShoppingListCountTextLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = ((System.Windows.Controls.TextBox)sender);
            var tuple = (Tuple<Blueprint, int>)textBox.Tag;

            if (!int.TryParse(textBox.Text, out var newValue) || newValue < 0)
            {
                textBox.Text = tuple.Item2.ToString();
            }
            else
            {
                var diff = newValue - tuple.Item2;
                viewModel.CurrentCommander.Value.ShoppingListChange(tuple.Item1, diff);
            }
        }

        private void SplashHidden(object sender, EventArgs e)
        {
            Splash.Visibility = Visibility.Collapsed;
        }

        private void BlueprintsSorting(object sender, DataGridSortingEventArgs e)
        {
            string realHeader;
            if (headers.ContainsKey(e.Column))
            {
                realHeader = headers[e.Column];
            }
            else
            {
                if (e.Column.Header is TextBlock textBlock)
                {
                    headers[e.Column] = realHeader = textBlock.Text;
                }
                else
                {
                    headers[e.Column] = realHeader = e.Column.Header.ToString();
                }
            }

            var view = CollectionViewSource.GetDefaultView(Blueprints.Items);
            var sortingOn = view.SortDescriptions.ToList();

            var path = e.Column.SortMemberPath;
            var currentSorting = sortingOn.FirstOrDefault(p => p.PropertyName == path);
            if (currentSorting.PropertyName != null)
            {
                int currentColumnSortRank;
                if (currentSorting.Direction == ListSortDirection.Descending && lastColumnSorted == e.Column)
                {
                    view.SortDescriptions.Clear();
                    foreach (var c in Blueprints.Columns)
                    {
                        if (headers.ContainsKey(c))
                        {
                            c.Header = headers[c];
                        }
                    }

                    sortedColumns = currentColumnSortRank = 1;
                }
                else
                {
                    currentColumnSortRank = e.Column.Header.ToString().Count(c => c == '↑' || c == '↓');
                    view.SortDescriptions.Remove(currentSorting);
                }

                ListSortDirection newDirection;
                string newHeader;
                switch (currentSorting.Direction)
                {
                    case ListSortDirection.Descending:
                        newDirection = ListSortDirection.Ascending;
                        var upArrows = string.Concat(Enumerable.Repeat("↑", currentColumnSortRank));
                        newHeader = $"{upArrows}{realHeader}";
                        break;
                    case ListSortDirection.Ascending:
                    default:
                        newDirection = ListSortDirection.Descending;
                        var downArrows = string.Concat(Enumerable.Repeat("↓", currentColumnSortRank));
                        newHeader = $"{downArrows}{realHeader}";
                        break;
                }

                e.Column.Header = newHeader;
                view.SortDescriptions.Add(new SortDescription(path, newDirection));
            }
            else
            {
                sortedColumns++;
                var upArrows = string.Concat(Enumerable.Repeat("↑", sortedColumns));
                e.Column.Header = $"{upArrows}{realHeader}";
                view.SortDescriptions.Add(new SortDescription(path, ListSortDirection.Ascending));
            }

            view.Refresh();
            lastColumnSorted = e.Column;
            e.Handled = true;
        }

        private int sortedColumns = 0;
        private DataGridColumn lastColumnSorted = null;
        private readonly Dictionary<DataGridColumn, string> headers = new Dictionary<DataGridColumn, string>();

       
    }
}