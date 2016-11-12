using System;
using System.Diagnostics;
using System.Windows.Forms;
using EDEngineer.Models.Localization;
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
            EventHandler selectLanguageHandler)
        {
            var menu = BuildContextMenu(showHandler, quitHandler, configureShortcutHandler, unlockWindowHandler, resetWindowHandler, selectLanguageHandler);
            
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
            EventHandler selectLanguageHandler)
        {
            var translator = Languages.Instance;

            var showItem = new MenuItem()
            {
                Text = translator.Translate("Show")
            };
            showItem.Click += showHandler;

            var unlockItem = new MenuItem()
            {
                Text = translator.Translate("Toggle Window Mode (Locked/Unlocked)")
            };
            unlockItem.Click += unlockWindowHandler;

            var resetItem = new MenuItem()
            {
                Text = translator.Translate("Reset Window Position")
            };
            resetItem.Click += resetWindowHandler;

            var selectLanguageItem = new MenuItem()
            {
                Text = translator.Translate("Select Language")
            };
            selectLanguageItem.Click += selectLanguageHandler;

            var setShortCutItem = new MenuItem()
            {
                Text = translator.Translate("Set Shortcut")
            };
            setShortCutItem.Click += configureShortcutHandler;

            var helpItem = new MenuItem()
            {
                Text = translator.Translate("Help")
            };
            helpItem.Click += (o,e) => Process.Start("https://github.com/msarilar/EDEngineer/wiki/Troubleshooting-Issues");

            var quitItem = new MenuItem()
            {
                Text = translator.Translate("Quit")
            };
            quitItem.Click += quitHandler;

            var menu = new ContextMenu();
            menu.MenuItems.Add(showItem);
            menu.MenuItems.Add(unlockItem);
            menu.MenuItems.Add(resetItem);
            menu.MenuItems.Add(setShortCutItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(selectLanguageItem);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(helpItem);
            menu.MenuItems.Add(quitItem);
            return menu;
        }
    }
}