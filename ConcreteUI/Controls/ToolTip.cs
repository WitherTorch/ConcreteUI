using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

namespace ConcreteUI.Controls
{
    public sealed class ToolTip : UIElement, IMouseEvents
    {
        private readonly CoreWindow window;
        private readonly Func<UIElement, bool>? checkingFunc;
        private readonly ConcurrentDictionary<UIElement, string> toolTipTextDict;
        private readonly System.Windows.Forms.ToolTip toolTip;

        private UIElement? _currentElement;
        private bool isPopup = false;

        public ToolTip(CoreWindow window, Func<UIElement, bool>? checkingFunc = null) : base(window, string.Empty)
        {
            this.window = window;
            this.checkingFunc = checkingFunc;
            toolTipTextDict = new ConcurrentDictionary<UIElement, string>();
            _currentElement = this;
            toolTip = new System.Windows.Forms.ToolTip()
            {
                ShowAlways = false,
            };
            toolTip.Popup += ToolTip_Popup;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider) { }

        protected override void OnThemePrefixChanged(string prefix) { }

        private void ToolTip_Popup(object? sender, System.Windows.Forms.PopupEventArgs e)
        {
            isPopup = true;
        }

        public void SetToolTip(string text)
        {
            SetToolTip(this, text);
        }

        public void SetToolTip(UIElement element, string text)
        {
            if (toolTipTextDict.TryGetValue(element, out string? oldText))
            {
                if (string.Equals(oldText, text, StringComparison.Ordinal))
                {
                    return;
                }
                else
                {
                    if (text is null)
                        toolTipTextDict.TryRemove(element, out _);
                    else
                        toolTipTextDict.TryUpdate(element, text, oldText);
                }
            }
            else
            {
                if (text is null)
                    return;
                toolTipTextDict.TryAdd(element, text);
            }
            if (_currentElement == element)
            {
                toolTip.SetToolTip(window, text);
            }
        }

        private void CheckToolTip(in PointF point)
        {
            string? fallbackText = null;
            Func<UIElement, bool>? checkingFunc = this.checkingFunc;
            foreach (KeyValuePair<UIElement, string> entry in toolTipTextDict)
            {
                UIElement element = entry.Key;
                if (checkingFunc?.Invoke(element) == false)
                    continue;
                string text = entry.Value;
                if (element == this)
                {
                    fallbackText = text;
                    continue;
                }
                if (element.Bounds.Contains(point))
                {
                    if (Interlocked.Exchange(ref _currentElement, element) != element)
                    {
                        toolTip.SetToolTip(window, text);
                    }
                    return;
                }
            }
            if (Interlocked.Exchange(ref _currentElement, this) != this)
            {
                toolTip.SetToolTip(window, fallbackText);
            }
        }

        public override void Render(DirtyAreaCollector collector) => ResetNeedRefreshFlag();

        protected override bool RenderCore(DirtyAreaCollector collector) => true;

        public void OnMouseDown(in MouseInteractEventArgs args)
        {
        }

        PointF lastPoint;
        public void OnMouseMove(in MouseInteractEventArgs args)
        {
            if (lastPoint != args.Location)
            {
                lastPoint = args.Location;

                if (isPopup)
                {
                    isPopup = false;
                    toolTip.SetToolTip(window, null);
                    Interlocked.Exchange(ref _currentElement, null);
                }
                CheckToolTip(args.Location);
            }
        }

        public void OnMouseUp(in MouseInteractEventArgs args)
        {
        }

    }
}
