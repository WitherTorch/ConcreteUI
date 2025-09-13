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

        private UIElement? _currentElement;
        private bool isPopup = false;

        public ToolTip(CoreWindow window, Func<UIElement, bool>? checkingFunc = null) : base(window, string.Empty)
        {
            this.window = window;
            this.checkingFunc = checkingFunc;
            toolTipTextDict = new ConcurrentDictionary<UIElement, string>();
            _currentElement = this;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider) { }

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
                //toolTip.SetToolTip(window, text);
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
                        //toolTip.SetToolTip(window, text);
                    }
                    return;
                }
            }
            if (Interlocked.Exchange(ref _currentElement, this) != this)
            {
                //toolTip.SetToolTip(window, fallbackText);
            }
        }

        public override void Render(in RegionalRenderingContext context) => ResetNeedRefreshFlag();

        protected override bool RenderCore(in RegionalRenderingContext context) => true;

        Point lastPoint;
        public void OnMouseMove(in MouseNotifyEventArgs args)
        {
            Point point = args.Location;
            if (lastPoint == point)
                return;
            lastPoint = point;

            if (isPopup)
            {
                isPopup = false;
               // toolTip.SetToolTip(window, null);
                Interlocked.Exchange(ref _currentElement, null);
            }
            CheckToolTip(point);
        }

        public void OnMouseUp(in MouseNotifyEventArgs args) { }

    }
}
