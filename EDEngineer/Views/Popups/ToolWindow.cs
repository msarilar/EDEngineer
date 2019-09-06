using System;
using System.Windows;
using System.Windows.Forms;
using MahApps.Metro.Controls;

namespace EDEngineer.Views.Popups
{
    public abstract class ToolWindow : MetroWindow
    {
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            MoveBottomRightEdgeOfWindowToMousePosition();
            Visibility = Visibility.Visible;
        }

        private void MoveBottomRightEdgeOfWindowToMousePosition()
        {
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(GetMousePosition());
            Left = mouse.X - ActualWidth;
            Top = mouse.Y - ActualHeight;
        }

        public Point GetMousePosition()
        {
            System.Drawing.Point point = Control.MousePosition;
            return new Point(point.X, point.Y);
        }
    }
}