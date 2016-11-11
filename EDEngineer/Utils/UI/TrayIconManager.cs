using System;
using System.Diagnostics;
using System.Windows.Forms;
using EDEngineer.Utils.System;

namespace EDEngineer.Utils.UI
{
    public static class TrayIconManager
    {
        public static IDisposable Init(EventHandler showHandler,
            EventHandler quitHandler,
            EventHandler configureShortcutHandler,
            EventHandler unlockWindowHandler,
            EventHandler resetWindowHandler)
        {
            var menu = BuildContextMenu(showHandler, quitHandler, configureShortcutHandler, unlockWindowHandler, resetWindowHandler);
            
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

        private static ContextMenu BuildContextMenu(EventHandler showHandler, EventHandler quitHandler, EventHandler configureShortcutHandler, EventHandler unlockWindowHandler, EventHandler resetWindowHandler)
        {
            var showItem = new MenuItem()
            {
                Text = "Show"
            };
            showItem.Click += showHandler;

            var unlockItem = new MenuItem()
            {
                Text = "Toggle Window Mode (Locked/Unlocked)"
            };
            unlockItem.Click += unlockWindowHandler;

            var resetItem = new MenuItem()
            {
                Text = "Reset Window Position"
            };
            resetItem.Click += resetWindowHandler;

            var setShortCutItem = new MenuItem()
            {
                Text = "Set Shortcut"
            };
            setShortCutItem.Click += configureShortcutHandler;

            var helpItem = new MenuItem()
            {
                Text = "Help",
            };
            helpItem.Click += (o,e) => Process.Start("https://github.com/msarilar/EDEngineer/wiki/Troubleshooting-Issues");

            var quitItem = new MenuItem()
            {
                Text = "Quit",
            };
            quitItem.Click += quitHandler;

            var menu = new ContextMenu();
            menu.MenuItems.Add(showItem);
            menu.MenuItems.Add(unlockItem);
            menu.MenuItems.Add(resetItem);
            menu.MenuItems.Add(setShortCutItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(helpItem);
            menu.MenuItems.Add(quitItem);
            return menu;
        }
    }
}