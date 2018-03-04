using System;
using System.Windows;
using System.Windows.Forms;
using EDEngineer.Models;
using EDEngineer.Views.Notifications;

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
                double height, width, left, top, leftSideWidth, rightSideWidth;
                if (Properties.Settings.Default.WindowHeight == 0 || Properties.Settings.Default.ResetUI)
                {
                    Properties.Settings.Default.WindowHeight = height = SystemParameters.PrimaryScreenHeight*0.6d;
                    Properties.Settings.Default.WindowWidth = width = SystemParameters.PrimaryScreenWidth*0.6d;
                    Properties.Settings.Default.WindowLeft = left = SystemParameters.PrimaryScreenWidth / 2d - width / 2d;
                    Properties.Settings.Default.WindowTop = top = SystemParameters.PrimaryScreenHeight / 2d - height / 2d;
                    Properties.Settings.Default.LeftSideWidth = leftSideWidth = 1;
                    Properties.Settings.Default.RightSideWidth = rightSideWidth = 1;
                    Properties.Settings.Default.ResetUI = false;

                    Properties.Settings.Default.Save();
                }
                else
                {
                    height = Properties.Settings.Default.WindowHeight;
                    width = Properties.Settings.Default.WindowWidth;
                    left = Properties.Settings.Default.WindowLeft;
                    top = Properties.Settings.Default.WindowTop;
                    leftSideWidth = Properties.Settings.Default.LeftSideWidth;
                    rightSideWidth = Properties.Settings.Default.RightSideWidth;
                }

                return new WindowDimensions { Height = height, Left = left, Top = top, Width = width, LeftSideWidth = leftSideWidth, RightSideWidth = rightSideWidth};
            }
            set
            {
                Properties.Settings.Default.WindowHeight = value.Height;
                Properties.Settings.Default.WindowWidth = value.Width;
                Properties.Settings.Default.WindowLeft = value.Left;
                Properties.Settings.Default.WindowTop = value.Top;
                Properties.Settings.Default.LeftSideWidth = value.LeftSideWidth;
                Properties.Settings.Default.RightSideWidth = value.RightSideWidth;

                Properties.Settings.Default.Save();
            }
        }

        public static bool SilentLaunch
        {
            get => Properties.Settings.Default.SilentLaunch;
            set
            {
                Properties.Settings.Default.SilentLaunch = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool IngredientsGrouped
        {
            get => Properties.Settings.Default.IngredientsGrouped;
            set
            {
                Properties.Settings.Default.IngredientsGrouped = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool SyncShoppingList
        {
            get => Properties.Settings.Default.SyncShoppingList;
            set
            {
                Properties.Settings.Default.SyncShoppingList = value;
                Properties.Settings.Default.Save();
            }
        }
        
        public static NotificationKind NotificationKindBlueprintReady
        {
            get => (NotificationKind)
                Enum.Parse(typeof (NotificationKind), Properties.Settings.Default.NotificationKindBlueprintReady);
            set
            {
                Properties.Settings.Default.NotificationKindBlueprintReady = value.ToString("G");
                Properties.Settings.Default.Save();
            }
        }

        public static string NotificationVoice
        {
            get => Properties.Settings.Default.NotificationVoice;
            set
            {
                Properties.Settings.Default.NotificationVoice = value;
                Properties.Settings.Default.Save();
            }
        }
        
        public static ushort ServerPort
        {
            get => Properties.Settings.Default.ServerPort;
            set
            {
                Properties.Settings.Default.ServerPort = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool AutoRunServer
        {
            get => Properties.Settings.Default.AutoRunServer;
            set
            {
                Properties.Settings.Default.AutoRunServer = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int CargoTabIndex
        {
            get => Properties.Settings.Default.CargoTabIndex;
            set
            {
                Properties.Settings.Default.CargoTabIndex = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string SelectedCommander
        {
            get => Properties.Settings.Default.SelectedCommander;
            set
            {
                Properties.Settings.Default.SelectedCommander = value;
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

        public static string Comparer
        {
            get
            {
                var comparer = Properties.Settings.Default.Comparer;
                if (string.IsNullOrEmpty(comparer))
                {
                    Comparer = comparer = State.NAME_COMPARER;
                }

                return comparer;
            }
            set
            {
                Properties.Settings.Default.Comparer = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
