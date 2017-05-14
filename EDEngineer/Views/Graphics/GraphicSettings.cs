using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Views.Graphics
{
    public class GraphicSettings : INotifyPropertyChanged
    {
        private float rightRatio;
        private float bottomRatio;
        private float leftRatio;

        public GraphicSettings()
        {
            leftRatio = Properties.Settings.Default.LeftFontRatio;
            rightRatio = Properties.Settings.Default.RightRatio;
            bottomRatio = Properties.Settings.Default.BottomFontRatio;
        }

        public void Sync()
        {
            Properties.Settings.Default.LeftFontRatio = leftRatio;
            Properties.Settings.Default.RightRatio = rightRatio;
            Properties.Settings.Default.BottomFontRatio = bottomRatio;

            Properties.Settings.Default.Save();
        }

        public void Reset()
        {
            LeftRatio = 100;
            RightRatio = 100;
            BottomRatio = 100;
        }

        public float LeftRatio
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

        public float RightRatio
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

        public float BottomRatio
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
