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
            rightRatio = Properties.Settings.Default.RightRatio;
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
            get
            {
                return opacity;
            }
            set
            {
                if (Math.Abs(opacity - value) < 0.01)
                {
                    return;
                }

                opacity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public double LeftRatio
        {
            get { return leftRatio; }
            set
            {
                if (Math.Abs(leftRatio - value) < 0.01)
                {
                    return;
                }

                leftRatio = value;
                OnPropertyChanged();
            }
        }

        public double RightRatio
        {
            get { return rightRatio; }
            set
            {
                if (Math.Abs(rightRatio - value) < 0.01)
                {
                    return;
                }

                rightRatio = value;
                OnPropertyChanged();
            }
        }

        public double BottomRatio
        {
            get { return bottomRatio; }
            set
            {
                if (Math.Abs(bottomRatio - value) < 0.01)
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
