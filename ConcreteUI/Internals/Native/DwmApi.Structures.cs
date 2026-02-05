using System;
using System.Runtime.InteropServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DWMBlurBehind
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
            readonly get => fTransitionOnMaximized;
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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct DwmTimingInfo
    {
        public uint cbSize;

        // Data on DWM composition overall

        /// <summary>
        /// Monitor refresh rate
        /// </summary>
        public Rational rateRefresh;

        /// <summary>
        /// Actual period
        /// </summary>
        public ulong qpcRefreshPeriod;

        /// <summary>
        /// composition rate
        /// </summary>
        public Rational rateCompose;

        /// <summary>
        /// QPC time at a VSync interupt
        /// </summary>
        public ulong qpcVBlank;

        /// <summary>
        /// DWM refresh count of the last vsync 
        /// </summary>
        /// <remarks>
        /// DWM refresh count is a 64bit number where zero is
        /// the first refresh the DWM woke up to process
        /// </remarks>
        public ulong cRefresh;

        /// <summary>
        /// DX refresh count at the last Vsync Interupt
        /// </summary>
        /// <remarks>
        /// DX refresh count is a 32bit number with zero
        /// being the first refresh after the card was initialized
        /// DX increments a counter when ever a VSync ISR is processed
        /// It is possible for DX to miss VSyncs<br/>
        /// There is not a fixed mapping between DX and DWM refresh counts
        /// because the DX will rollover and may miss VSync interupts
        /// </remarks>
        public uint cDXRefresh;

        /// <summary>
        /// QPC time at a compose time.
        /// </summary>
        public ulong qpcCompose;

        /// <summary>
        /// Frame number that was composed at qpcCompose
        /// </summary>
        public ulong cFrame;

        /// <summary>
        /// The present number DX uses to identify renderer frames
        /// </summary>
        public uint cDXPresent;

        /// <summary>
        /// Refresh count of the frame that was composed at qpcCompose
        /// </summary>
        public ulong cRefreshFrame;


        /// <summary>
        /// DWM frame number that was last submitted
        /// </summary>
        public ulong cFrameSubmitted;

        /// <summary>
        /// DX Present number that was last submitted
        /// </summary>
        public uint cDXPresentSubmitted;

        /// <summary>
        /// DWM frame number that was last confirmed presented
        /// </summary>
        public ulong cFrameConfirmed;

        /// <summary>
        /// DX Present number that was last confirmed presented
        /// </summary>
        public uint cDXPresentConfirmed;

        /// <summary>
        /// The target refresh count of the last
        /// frame confirmed completed by the GPU
        /// </summary>
        public ulong cRefreshConfirmed;

        /// <summary>
        /// DX refresh count when the frame was confirmed presented
        /// </summary>
        public uint cDXRefreshConfirmed;

        /// <summary>
        /// Number of frames the DWM presented late
        /// AKA Glitches
        /// </summary>
        public ulong cFramesLate;

        /// <summary>
        /// the number of composition frames that
        /// have been issued but not confirmed completed
        /// </summary>
        public uint cFramesOutstanding;


        // Following fields are only relavent when an HWND is specified
        // Display frame


        /// <summary>
        /// Last frame displayed
        /// </summary>
        public ulong cFrameDisplayed;

        /// <summary>
        /// QPC time of the composition pass when the frame was displayed
        /// </summary>
        public ulong qpcFrameDisplayed;

        /// <summary>
        /// Count of the VSync when the frame should have become visible
        /// </summary>
        public ulong cRefreshFrameDisplayed;

        // Complete frames: DX has notified the DWM that the frame is done rendering

        /// <summary>
        /// ID of the the last frame marked complete (starts at 0)
        /// </summary>
        public ulong cFrameComplete;

        /// <summary>
        /// QPC time when the last frame was marked complete
        /// </summary>
        public ulong qpcFrameComplete;

        // Pending frames:
        // The application has been submitted to DX but not completed by the GPU

        /// <summary>
        /// ID of the the last frame marked pending (starts at 0)
        /// </summary>
        public ulong cFramePending;

        /// <summary>
        /// QPC time when the last frame was marked pending
        /// </summary>
        public ulong qpcFramePending;

        // number of unique frames displayed
        public ulong cFramesDisplayed;

        /// <summary>
        /// number of new completed frames that have been received
        /// </summary>
        public ulong cFramesComplete;

        /// <summary>
        /// number of new frames submitted to DX but not yet complete
        /// </summary>
        public ulong cFramesPending;

        /// <summary>
        /// number of frames available but not displayed, used or dropped
        /// </summary>
        public ulong cFramesAvailable;

        /// <summary>
        /// number of rendered frames that were never
        /// displayed because composition occured too late
        /// </summary>
        public ulong cFramesDropped;

        /// <summary>
        /// number of times an old frame was composed
        /// when a new frame should have been used
        /// but was not available
        /// </summary>
        public ulong cFramesMissed;

        /// <summary>
        /// the refresh at which the next frame is
        /// scheduled to be displayed
        /// </summary>
        public ulong cRefreshNextDisplayed;

        /// <summary>
        /// the refresh at which the next DX present is
        /// scheduled to be displayed
        /// </summary>
        public ulong cRefreshNextPresented;

        /// <summary>
        /// The total number of refreshes worth of content
        /// for this HWND that have been displayed by the DWM
        /// since DwmSetPresentParameters was called
        /// </summary>
        public ulong cRefreshesDisplayed;

        /// <summary>
        /// The total number of refreshes worth of content
        /// that have been presented by the application
        /// since DwmSetPresentParameters was called
        /// </summary>
        public ulong cRefreshesPresented;

        /// <summary>
        /// The actual refresh # when content for this
        /// window started to be displayed
        /// </summary>
        /// <remarks>
        /// it may be different than that requested
        /// DwmSetPresentParameters
        /// </remarks>
        public ulong cRefreshStarted;

        /// <summary>
        /// Total number of pixels DX redirected
        /// to the DWM.
        /// </summary>
        /// <remarks>
        /// If Queueing is used the full buffer
        /// is transfered on each present. <br/>
        /// If not queuing it is possible only
        /// a dirty region is updated
        ///</remarks>
        public ulong cPixelsReceived;

        /// <summary>
        /// Total number of pixels drawn.
        /// </summary>
        /// <remarks>
        /// Does not take into account if
        /// if the window is only partial drawn
        /// do to clipping or dirty rect management
        /// </remarks>
        public ulong cPixelsDrawn;

        /// <summary>
        /// The number of buffers in the flipchain that are empty.<br/>
        /// An application can
        /// present that number of times and guarantee
        /// it won't be blocked waiting for a buffer to
        /// become empty to present to
        /// </summary>
        public ulong cBuffersEmpty;
    }
}
