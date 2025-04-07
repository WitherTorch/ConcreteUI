using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ConcreteUI.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public readonly ref struct MouseInteractEventArgs
    {
        public readonly PointF Location;
        public readonly float X;
        public readonly float Y;
        public readonly MouseButtons Button;
        public readonly int Delta;

        public MouseInteractEventArgs(PointF point)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = MouseButtons.None;
            Delta = 0;
        }

        public MouseInteractEventArgs(PointF point, MouseButtons buttons)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = buttons;
            Delta = 0;
        }

        public MouseInteractEventArgs(PointF point, int delta)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = MouseButtons.None;
            Delta = delta;
        }

        public MouseInteractEventArgs(PointF point, MouseButtons buttons, int delta)
        {
            Location = point;
            X = point.X;
            Y = point.Y;
            Button = buttons;
            Delta = delta;
        }
    }

}
