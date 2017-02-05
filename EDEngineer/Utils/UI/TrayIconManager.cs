using System;
using System.Diagnostics;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Utils.System;

namespace EDEngineer.Utils.UI
{
    public static class TrayIconManager
    {
        public static IDisposable Init(EventHandler showHandler,
            EventHandler quitHandler,
            EventHandler configureShortcutHandler,
            EventHandler unlockWindowHandler,
            EventHandler resetWindowHandler,
            EventHandler selectLanguageHandler,
            Func<bool> launchServerHandler,
            bool serverRunning,
            EventHandler showReleaseNotesHandler,
            string version,
            EventHandler configureThresholdsHandler)
        {
            var menu = BuildContextMenu(showHandler,
                quitHandler,
                configureShortcutHandler,
                unlockWindowHandler,
                resetWindowHandler, 
                selectLanguageHandler,
                launchServerHandler,
                serverRunning,
                showReleaseNotesHandler,
                version,
                configureThresholdsHandler);
            
            var icon = new NotifyIcon
            {
                Icon = Properties.Resources.elite_dangerous_icon,
                Visible = true,
                Text = "ED - Engineer",
                ContextMenu = menu
            };

            return Disposable.Create(() =>
            {
                icon.Visible = false;
                icon.Icon = null;
                icon.Dispose();
            });
        }

        private static ContextMenu BuildContextMenu(EventHandler showHandler, 
            EventHandler quitHandler, 
            EventHandler configureShortcutHandler, 
            EventHandler unlockWindowHandler, 
            EventHandler resetWindowHandler,
            EventHandler selectLanguageHandler,
            Func<bool> launchServerHandler,
            bool serverRunning,
            EventHandler showReleaseNotesHandler,
            string version,
            EventHandler configureThresholdsHandler)
        {
            var translator = Languages.Instance;

            var showItem = new MenuItem();
            showItem.Click += showHandler;

            var unlockItem = new MenuItem();
            unlockItem.Click += unlockWindowHandler;

            var resetItem = new MenuItem();
            resetItem.Click += resetWindowHandler;

            var selectLanguageItem = new MenuItem();
            selectLanguageItem.Click += selectLanguageHandler;

            var setShortCutItem = new MenuItem();
            setShortCutItem.Click += configureShortcutHandler;

            var configureThresholdsItem = new MenuItem()
            {
                Enabled = Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)
            };
            configureThresholdsItem.Click += configureThresholdsHandler;

            var helpItem = new MenuItem();
            helpItem.Click += (o,e) => Process.Start("https://github.com/msarilar/EDEngineer/wiki/Troubleshooting-Issues");

            var releaseNotesItem = new MenuItem();
            releaseNotesItem.Click += showReleaseNotesHandler;
            releaseNotesItem.Text = $"v{version}";

            var quitItem = new MenuItem();
 
            var enableBlueprintReadyItem = new MenuItem
            {
                Checked = SettingsManager.BlueprintReadyToastEnabled && Environment.OSVersion.Version >= new Version(6, 2, 9200, 0),
                Enabled = Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)
            };

            enableBlueprintReadyItem.Click += (o, e) =>
            {
                enableBlueprintReadyItem.Checked = !enableBlueprintReadyItem.Checked;
                SettingsManager.BlueprintReadyToastEnabled = enableBlueprintReadyItem.Checked;
            };

            var enableCargoFullWarningItem = new MenuItem
            {
                Checked = SettingsManager.CargoAlmostFullWarningEnabled && Environment.OSVersion.Version >= new Version(6, 2, 9200, 0),
                Enabled = Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)
            };

            enableCargoFullWarningItem.Click += (o, e) =>
            {
                enableCargoFullWarningItem.Checked = !enableCargoFullWarningItem.Checked;
                SettingsManager.CargoAlmostFullWarningEnabled = enableCargoFullWarningItem.Checked;
            };

            var launchServerItem = new MenuItem
            {
                Checked = serverRunning
            };

            launchServerItem.Click += (o, e) =>
            {
                launchServerItem.Checked = launchServerHandler();
            };

            SetItemsText(quitItem, translator, helpItem, setShortCutItem, selectLanguageItem, resetItem, unlockItem, showItem, enableBlueprintReadyItem, enableCargoFullWarningItem, launchServerItem, configureThresholdsItem);
            translator.PropertyChanged += (o, e) =>
            {
                SetItemsText(quitItem, translator, helpItem, setShortCutItem, selectLanguageItem, resetItem, unlockItem, showItem, enableBlueprintReadyItem, enableCargoFullWarningItem, launchServerItem, configureThresholdsItem);
            };

            quitItem.Click += quitHandler;

            var menu = new ContextMenu();
            menu.MenuItems.Add(showItem);
            menu.MenuItems.Add(unlockItem);
            menu.MenuItems.Add(resetItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(enableCargoFullWarningItem);
            menu.MenuItems.Add(enableBlueprintReadyItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(setShortCutItem);
            menu.MenuItems.Add(selectLanguageItem);
            menu.MenuItems.Add(configureThresholdsItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(launchServerItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(helpItem);
            menu.MenuItems.Add(releaseNotesItem);
            menu.MenuItems.Add(quitItem);
            return menu;
        }

        private static void SetItemsText(MenuItem quitItem, Languages translator, MenuItem helpItem, MenuItem setShortCutItem,
                                         MenuItem selectLanguageItem, MenuItem resetItem, MenuItem unlockItem, MenuItem showItem,
                                         MenuItem enableToastsItem, MenuItem enableCargoFullWarningItem, MenuItem launchServerItem,
                                         MenuItem configureThresholdsItem)
        {
            quitItem.Text = translator.Translate("Quit");
            helpItem.Text = translator.Translate("Help");
            setShortCutItem.Text = translator.Translate("Set Shortcut");
            selectLanguageItem.Text = translator.Translate("Select Language");
            resetItem.Text = translator.Translate("Reset Window Position");
            unlockItem.Text = translator.Translate("Toggle Window Mode (Locked/Unlocked)");
            showItem.Text = translator.Translate("Show");
            enableToastsItem.Text = translator.Translate("Blueprint Ready");
            enableCargoFullWarningItem.Text = translator.Translate("Cargo Almost Full Warning");
            launchServerItem.Text = translator.Translate("Launch Local API");
            configureThresholdsItem.Text = translator.Translate("Configure Thresholds");
        }
    }
}