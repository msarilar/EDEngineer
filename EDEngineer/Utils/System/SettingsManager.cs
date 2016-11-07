using System.Windows;
using System.Windows.Forms;

namespace EDEngineer.Utils.System
{
    public static class SettingsManager
    {
        public static void Init()
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;

                Properties.Settings.Default.Save();
            }
        }

        public static void Reset(this WindowDimensions dimensions)
        {
            Properties.Settings.Default.WindowHeight = 0;
            Properties.Settings.Default.Save();
        }

        public static WindowDimensions Dimensions
        {
            get
            {
                double height, width, left, top;
                if (Properties.Settings.Default.WindowHeight == 0)
                {
                    Properties.Settings.Default.WindowHeight = height = SystemParameters.PrimaryScreenHeight*0.6d;
                    Properties.Settings.Default.WindowWidth = width = SystemParameters.PrimaryScreenWidth*0.6d;
                    Properties.Settings.Default.WindowLeft = left = SystemParameters.PrimaryScreenWidth / 2d - width / 2d;
                    Properties.Settings.Default.WindowTop = top = SystemParameters.PrimaryScreenHeight / 2d - height / 2d;

                    Properties.Settings.Default.Save();
                }
                else
                {
                    height = Properties.Settings.Default.WindowHeight;
                    width = Properties.Settings.Default.WindowWidth;
                    left = Properties.Settings.Default.WindowLeft;
                    top = Properties.Settings.Default.WindowTop;
                }

                return new WindowDimensions() { Height = height, Left = left, Top = top, Width = width };
            }
            set
            {
                Properties.Settings.Default.WindowHeight = value.Height;
                Properties.Settings.Default.WindowWidth = value.Width;
                Properties.Settings.Default.WindowLeft = value.Left;
                Properties.Settings.Default.WindowTop = value.Top;

                Properties.Settings.Default.Save();
            }
        }

        public static string Shortcut
        {
            get
            {
                var shortcut = Properties.Settings.Default.Shortcut;
                if (string.IsNullOrEmpty(shortcut))
                {
                    var converter = new KeysConverter();
                    shortcut = Properties.Settings.Default.Shortcut = converter.ConvertToString(Keys.F10 | Keys.Control);
                    Properties.Settings.Default.Save();
                }

                return shortcut;
            }
            set
            {
                Properties.Settings.Default.Shortcut = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
