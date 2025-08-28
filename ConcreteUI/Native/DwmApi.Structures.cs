using System;
using System.Runtime.InteropServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DWMBlurBehind
    {
        public DwmBlurBehindFlags dwFlags;
        public SysBool fEnable;
        public IntPtr hRgnBlur;
        public SysBool fTransitionOnMaximized;

        public DWMBlurBehind(bool enabled)
        {
            fEnable = enabled;
            hRgnBlur = IntPtr.Zero;
            fTransitionOnMaximized = false;
            dwFlags = DwmBlurBehindFlags.Enable;
        }

        public readonly System.Drawing.Region Region 
            => System.Drawing.Region.FromHrgn(hRgnBlur);

        public bool TransitionOnMaximized
        {
            get => fTransitionOnMaximized;
            set
            {
                fTransitionOnMaximized = value;
                dwFlags |= DwmBlurBehindFlags.TransitionMaximized;
            }
        }

        public void SetRegion(System.Drawing.Graphics graphics, System.Drawing.Region region)
        {
            hRgnBlur = region.GetHrgn(graphics);
            dwFlags |= DwmBlurBehindFlags.BlurRegion;
        }
    }
}
