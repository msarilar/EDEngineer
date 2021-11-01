using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Views.Popups.Graphics
{
    public class GraphicSettings : INotifyPropertyChanged
    {
        private double rightRatio;
        private double bottomRatio;
        private double leftRatio;
        private double opacity;

        public GraphicSettings()
        {
            leftRatio = Properties.Settings.Default.LeftFontRatio;
            rightRatio = Math.Max(61, Properties.Settings.Default.RightRatio);
            bottomRatio = Properties.Settings.Default.BottomFontRatio;
            opacity = Properties.Settings.Default.Opacity;
        }

        public void Reset(double left, double right, double bottom, double o)
        {
            LeftRatio = left;
            RightRatio = right;
            BottomRatio = bottom;
            OpacityForEdit = o;
        }

        public void Save()
        {
            Properties.Settings.Default.LeftFontRatio = leftRatio;
            Properties.Settings.Default.RightRatio = rightRatio;
            Properties.Settings.Default.BottomFontRatio = bottomRatio;
            Properties.Settings.Default.Opacity = opacity;

            Properties.Settings.Default.Save();
        }

        public double Opacity
        {
            get
            {
                if (Properties.Settings.Default.WindowUnlocked)
                {
                    return 1;
                }

                return opacity;
            }
        }

        public double OpacityForEdit
        {
            get => opacity;
            set
            {
                if (value < 0.1)
                {
                    opacity = 0.1;
                    return;
                }

                opacity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public double LeftRatio
        {
            get => leftRatio;
            set
            {
                if (Math.Abs(leftRatio - value) < 1)
                {
                    return;
                }

                if (Math.Abs(value) < 1)
                {
                    return;
                }

                leftRatio = value;
                OnPropertyChanged();
            }
        }

        public double RightRatio
        {
            get => rightRatio;
            set
            {
                if (Math.Abs(rightRatio - value) < 1)
                {
                    return;
                }

                value = Math.Max(61, value);

                rightRatio = value;
                OnPropertyChanged();
            }
        }

        public double BottomRatio
        {
            get => bottomRatio;
            set
            {
                if (Math.Abs(bottomRatio - value) < 0.01)
                {
                    return;
                }

                if (Math.Abs(value) < 0.1)
                {
                    return;
                }

                bottomRatio = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
