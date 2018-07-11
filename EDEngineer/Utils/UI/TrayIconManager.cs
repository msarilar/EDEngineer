using System;
using System.Diagnostics;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Utils.System;

namespace EDEngineer.Utils.UI
{
    public static class TrayIconManager
    {
        public static IDisposable Init(ContextMenuStrip menu)
        {
            var icon = new NotifyIcon
            {
                Icon = Properties.Resources.elite_dangerous_icon,
                Visible = true,
                Text = "ED - Engineer",
                ContextMenuStrip = menu
            };

            return Disposable.Create(() =>
            {
                icon.Visible = false;
                icon.Icon = null;
                icon.Dispose();
            });
        }

        public static ContextMenuStrip BuildContextMenu(EventHandler showHandler, 
            EventHandler quitHandler, 
            EventHandler configureShortcutHandler, 
            EventHandler unlockWindowHandler, 
            EventHandler resetWindowHandler,
            EventHandler selectLanguageHandler,
            Func<bool> launchServerHandler,
            bool serverRunning,
            EventHandler showReleaseNotesHandler,
            string version,
            EventHandler configureNotificationsHandler,
            EventHandler configureGraphicsHandler,
            EventHandler openChartHandler)
        {
            var translator = Languages.Instance;

            var showItem = new ToolStripMenuItem { Image = Properties.Resources.elite_dangerous_icon.ToBitmap() };
            showItem.Click += showHandler;

            var unlockItem = new ToolStripMenuItem { Image = Properties.Resources.menu_lock_unlock.ToBitmap() };
            unlockItem.Click += unlockWindowHandler;

            var resetItem = new ToolStripMenuItem { Image = Properties.Resources.menu_reset_position.ToBitmap() };
            resetItem.Click += resetWindowHandler;

            var selectLanguageItem = new ToolStripMenuItem { Image = Properties.Resources.menu_language.ToBitmap() };
            selectLanguageItem.Click += selectLanguageHandler;

            var configureGraphicsItem = new ToolStripMenuItem
            {
                Image = Properties.Resources.menu_graphic_settings.ToBitmap()
            };
            configureGraphicsItem.Click += configureGraphicsHandler;

            var setShortCutItem = new ToolStripMenuItem { Image = Properties.Resources.menu_shortcut.ToBitmap() };
            setShortCutItem.Click += configureShortcutHandler;

            var configureNotificationsItem = new ToolStripMenuItem
            {
                Image = Properties.Resources.menu_notifications.ToBitmap()
            };
            configureNotificationsItem.Click += configureNotificationsHandler;

            var helpItem = new ToolStripMenuItem { Image = Properties.Resources.menu_help.ToBitmap() };
            helpItem.Click += (o,e) => Process.Start("https://github.com/msarilar/EDEngineer/wiki/Troubleshooting-Issues");

            var releaseNotesItem = new ToolStripMenuItem { Image = Properties.Resources.menu_release_notes.ToBitmap() };
            releaseNotesItem.Click += showReleaseNotesHandler;
            releaseNotesItem.Text = $"v{version}";

            var quitItem = new ToolStripMenuItem { Image = Properties.Resources.menu_quit.ToBitmap() };

            var enableSilentLaunch = new ToolStripMenuItem
            {
                Checked = SettingsManager.SilentLaunch,
                Image = Properties.Resources.menu_silent_launch.ToBitmap()
            };

            enableSilentLaunch.Click += (o, e) =>
            {
                enableSilentLaunch.Checked = !enableSilentLaunch.Checked;
                SettingsManager.SilentLaunch = enableSilentLaunch.Checked;
            };

            var openChartItem = new ToolStripMenuItem
            {
                Image = Properties.Resources.menu_chart.ToBitmap(),
                Enabled = serverRunning
            };

            var launchServerItem = new ToolStripMenuItem
            {
                Checked = serverRunning,
                Image = Properties.Resources.menu_api.ToBitmap()
            };

            launchServerItem.Click += (o, e) =>
            {
                openChartItem.Enabled = launchServerItem.Checked = launchServerHandler();
            };

            openChartItem.Click += openChartHandler;

            SetItemsText(quitItem, translator, helpItem, setShortCutItem, selectLanguageItem, resetItem, unlockItem, showItem, launchServerItem, enableSilentLaunch, configureNotificationsItem, configureGraphicsItem, openChartItem);
            translator.PropertyChanged += (o, e) =>
            {
                SetItemsText(quitItem, translator, helpItem, setShortCutItem, selectLanguageItem, resetItem, unlockItem, showItem, launchServerItem, enableSilentLaunch, configureNotificationsItem, configureGraphicsItem, openChartItem);
            };

            quitItem.Click += quitHandler;

            var menu = new ContextMenuStrip();
            menu.Items.Add(showItem);
            menu.Items.Add(unlockItem);
            menu.Items.Add(resetItem);
            menu.Items.Add("-");
            menu.Items.Add(configureNotificationsItem);
            menu.Items.Add("-");
            menu.Items.Add(enableSilentLaunch);
            menu.Items.Add(setShortCutItem);
            menu.Items.Add(selectLanguageItem);
            menu.Items.Add(configureGraphicsItem);
            menu.Items.Add("-");
            menu.Items.Add(launchServerItem);
            menu.Items.Add(openChartItem);
            menu.Items.Add("-");
            menu.Items.Add(helpItem);
            menu.Items.Add(releaseNotesItem);
            menu.Items.Add(quitItem);
            return menu;
        }

        private static void SetItemsText(ToolStripMenuItem quitItem, Languages translator, ToolStripMenuItem helpItem, ToolStripMenuItem setShortCutItem,
                                         ToolStripMenuItem selectLanguageItem, ToolStripMenuItem resetItem, ToolStripMenuItem unlockItem, ToolStripMenuItem showItem,
                                         ToolStripMenuItem launchServerItem, ToolStripMenuItem enableSilentLaunch, ToolStripMenuItem configureNotificationsItem,
                                         ToolStripMenuItem configureGraphicsItem, ToolStripMenuItem openChartItem)
        {
            quitItem.Text = translator.Translate("Quit");
            helpItem.Text = translator.Translate("Help");
            setShortCutItem.Text = translator.Translate("Set Shortcut");
            selectLanguageItem.Text = translator.Translate("Select Language");
            resetItem.Text = translator.Translate("Reset Window Position");
            unlockItem.Text = translator.Translate("Toggle Window Mode (Locked/Unlocked)");
            showItem.Text = translator.Translate("Show");
            launchServerItem.Text = translator.Translate("Launch Local API");
            enableSilentLaunch.Text = translator.Translate("Silent Launch");
            configureNotificationsItem.Text = translator.Translate("Configure Notifications");
            configureGraphicsItem.Text = translator.Translate("Configure Graphics");
            openChartItem.Text = translator.Translate("Cargo History (require API)");
        }
    }
}