using System;
using System.Runtime.InteropServices;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DWMBlurBehind
    {
        public DWMBlurBehindFlags dwFlags;
        public bool fEnable;
        public IntPtr hRgnBlur;
        public bool fTransitionOnMaximized;

        public DWMBlurBehind(bool enabled)
        {
            fEnable = enabled;
            hRgnBlur = IntPtr.Zero;
            fTransitionOnMaximized = false;
            dwFlags = DWMBlurBehindFlags.Enable;
        }

        public System.Drawing.Region Region
        {
            get { return System.Drawing.Region.FromHrgn(hRgnBlur); }
        }

        public bool TransitionOnMaximized
        {
            get => fTransitionOnMaximized;
            set
            {
                fTransitionOnMaximized = value;
                dwFlags |= DWMBlurBehindFlags.TransitionMaximized;
            }
        }

        public void SetRegion(System.Drawing.Graphics graphics, System.Drawing.Region region)
        {
            hRgnBlur = region.GetHrgn(graphics);
            dwFlags |= DWMBlurBehindFlags.BlurRegion;
        }
    }
}
