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
            EventHandler configureThresholdsHandler,
            EventHandler configureNotificationsHandler,
            EventHandler configureGraphicsHandler)
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
                configureThresholdsHandler,
                configureNotificationsHandler,
                configureGraphicsHandler);
            
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
            EventHandler configureThresholdsHandler,
            EventHandler configureNotificationsHandler,
            EventHandler configureGraphicsHandler)
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

            var configureGraphicsItem = new MenuItem();
            configureGraphicsItem.Click += configureGraphicsHandler;

            var setShortCutItem = new MenuItem();
            setShortCutItem.Click += configureShortcutHandler;

            var configureThresholdsItem = new MenuItem();
            configureThresholdsItem.Click += configureThresholdsHandler;

            var configureNotificationsItem = new MenuItem();
            configureNotificationsItem.Click += configureNotificationsHandler;

            var helpItem = new MenuItem();
            helpItem.Click += (o,e) => Process.Start("https://github.com/msarilar/EDEngineer/wiki/Troubleshooting-Issues");

            var releaseNotesItem = new MenuItem();
            releaseNotesItem.Click += showReleaseNotesHandler;
            releaseNotesItem.Text = $"v{version}";

            var quitItem = new MenuItem();

            var enableSilentLaunch = new MenuItem
            {
                Checked = SettingsManager.SilentLaunch
            };

            enableSilentLaunch.Click += (o, e) =>
            {
                enableSilentLaunch.Checked = !enableSilentLaunch.Checked;
                SettingsManager.SilentLaunch = enableSilentLaunch.Checked;
            };

            var launchServerItem = new MenuItem
            {
                Checked = serverRunning
            };

            launchServerItem.Click += (o, e) =>
            {
                launchServerItem.Checked = launchServerHandler();
            };

            SetItemsText(quitItem, translator, helpItem, setShortCutItem, selectLanguageItem, resetItem, unlockItem, showItem, launchServerItem, configureThresholdsItem, enableSilentLaunch, configureNotificationsItem, configureGraphicsItem);
            translator.PropertyChanged += (o, e) =>
            {
                SetItemsText(quitItem, translator, helpItem, setShortCutItem, selectLanguageItem, resetItem, unlockItem, showItem, launchServerItem, configureThresholdsItem, enableSilentLaunch, configureNotificationsItem, configureGraphicsItem);
            };

            quitItem.Click += quitHandler;

            var menu = new ContextMenu();
            menu.MenuItems.Add(showItem);
            menu.MenuItems.Add(unlockItem);
            menu.MenuItems.Add(resetItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(configureNotificationsItem);
            menu.MenuItems.Add(configureThresholdsItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(enableSilentLaunch);
            menu.MenuItems.Add(setShortCutItem);
            menu.MenuItems.Add(selectLanguageItem);
            menu.MenuItems.Add(configureGraphicsItem);
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
                                         MenuItem launchServerItem, MenuItem configureThresholdsItem, MenuItem enableSilentLaunch,
                                         MenuItem configureNotificationsItem, MenuItem configureGraphicsItem)
        {
            quitItem.Text = translator.Translate("Quit");
            helpItem.Text = translator.Translate("Help");
            setShortCutItem.Text = translator.Translate("Set Shortcut");
            selectLanguageItem.Text = translator.Translate("Select Language");
            resetItem.Text = translator.Translate("Reset Window Position");
            unlockItem.Text = translator.Translate("Toggle Window Mode (Locked/Unlocked)");
            showItem.Text = translator.Translate("Show");
            launchServerItem.Text = translator.Translate("Launch Local API");
            configureThresholdsItem.Text = translator.Translate("Configure Thresholds");
            enableSilentLaunch.Text = translator.Translate("Silent Launch");
            configureNotificationsItem.Text = translator.Translate("Configure Notifications");
            configureGraphicsItem.Text = translator.Translate("Configure Graphics");
        }
    }
}