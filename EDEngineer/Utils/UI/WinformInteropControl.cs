using System.Windows;
using System.Windows.Forms;

namespace EDEngineer.Utils.UI
{
    public static class WinformInteropControl
    {
        private static readonly Form winformFormHelper = new Form
        {
            Width = 1,
            Height = 1,
            AllowTransparency = true,
            Opacity = 0,
            ShowInTaskbar = false
        };

        static WinformInteropControl()
        {
            winformFormHelper.Show();
        }

        public static Control GetAtPoint(Point p)
        {
            winformFormHelper.Location = new global::System.Drawing.Point((int)p.X, (int)p.Y);
            return winformFormHelper;
        }
    }
}