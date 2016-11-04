using System.Windows.Forms;

namespace EDEngineer.Utils.System
{
    public class SettingsManager
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
