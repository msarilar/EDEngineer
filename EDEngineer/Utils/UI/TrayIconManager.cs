using System;
using System.Windows.Forms;
using EDEngineer.Utils.System;

namespace EDEngineer.Utils.UI
{
    public static class TrayIconManager
    {
        public static IDisposable Init(EventHandler showHandler,
            EventHandler quitHandler,
            EventHandler configureShortcutHandler,
            EventHandler unlockWindowHandler)
        {
            var menu = BuildContextMenu(showHandler, quitHandler, configureShortcutHandler, unlockWindowHandler);
            
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

        private static ContextMenu BuildContextMenu(EventHandler showHandler, EventHandler quitHandler, EventHandler configureShortcutHandler, EventHandler unlockWindowHandler)
        {
            var showItem = new MenuItem()
            {
                Text = "Show"
            };
            showItem.Click += showHandler;

            var unlockItem = new MenuItem()
            {
                Text = "Toggle Window Mode (Lock/Unlocked)"
            };
            unlockItem.Click += unlockWindowHandler;

            var setShortCutItem = new MenuItem()
            {
                Text = "Set Shortcut"
            };
            setShortCutItem.Click += configureShortcutHandler;
            var quitItem = new MenuItem()
            {
                Text = "Quit",
            };
            quitItem.Click += quitHandler;

            var menu = new ContextMenu();
            menu.MenuItems.Add(showItem);
            menu.MenuItems.Add(unlockItem);
            menu.MenuItems.Add(setShortCutItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(quitItem);
            return menu;
        }
    }
}