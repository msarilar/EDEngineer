using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace EDEngineer.Utils.System
{
    public static class HotkeyManager
    {
        private const int MOD_ALT = 0x1;
        private const int MOD_CONTROL = 0x2;
        private const int MOD_SHIFT = 0x4;
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] int fsModifiers, [In] int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);
        
        public static void RegisterHotKey(Window window, Keys key)
        {
            var modifiers = 0;

            if ((key & Keys.Alt) == Keys.Alt)
                modifiers = modifiers | MOD_ALT;

            if ((key & Keys.Control) == Keys.Control)
                modifiers = modifiers | MOD_CONTROL;

            if ((key & Keys.Shift) == Keys.Shift)
                modifiers = modifiers | MOD_SHIFT;

            var filteredKeys = key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
            RegisterHotKey(new WindowInteropHelper(window).Handle, 42, modifiers, (int)filteredKeys);
        }

        public static void UnregisterHotKey(Window window)
        {
            try
            {
                UnregisterHotKey(new WindowInteropHelper(window).Handle, 42);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}