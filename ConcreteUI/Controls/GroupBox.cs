﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class GroupBox : UIElement, IDisposable, IContainerElement
    {
        private static readonly string[] BrushNamesTemplate = new string[(int)Brush._Last]
        {
            "back",
            "border",
            "fore"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly string[] _brushNames = new string[(int)Brush._Last];
        private readonly LazyTiny<LayoutVariable>[] _contentLayoutReferences;
        private readonly ObservableList<UIElement> _children;

        private LazyTiny<LayoutVariable> _textTopVariableLazy;
        private DWriteTextLayout? _titleLayout, _textLayout;
        private string? _fontName;
        private string _title, _text;
        private long _redrawTypeRaw, _rawUpdateFlags;
        private int _titleHeight;
        private bool _disposed;

        public GroupBox(IRenderer renderer) : base(renderer, "app.groupBox")
        {
            _children = new ObservableList<UIElement>(new UnwrappableList<UIElement>(capacity: 0));
            _children.BeforeAdd += Children_BeforeAdded;
            _title = string.Empty;
            _text = string.Empty;
            _redrawTypeRaw = (long)RedrawType.RedrawAllContent;
            _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;

            LazyTiny<LayoutVariable>[] contentLayoutVariables = new LazyTiny<LayoutVariable>[(int)LayoutProperty._Last];
            contentLayoutVariables[(int)LayoutProperty.Left] = new LazyTiny<LayoutVariable>(() => new ContentLeftVariable(this), LazyThreadSafetyMode.PublicationOnly);
            contentLayoutVariables[(int)LayoutProperty.Top] = new LazyTiny<LayoutVariable>(() => new ContentTopVariable(this), LazyThreadSafetyMode.PublicationOnly);
            contentLayoutVariables[(int)LayoutProperty.Right] = new LazyTiny<LayoutVariable>(() => new ContentRightVariable(this), LazyThreadSafetyMode.PublicationOnly);
            contentLayoutVariables[(int)LayoutProperty.Bottom] = new LazyTiny<LayoutVariable>(() => new ContentBottomVariable(this), LazyThreadSafetyMode.PublicationOnly);
            contentLayoutVariables[(int)LayoutProperty.Width] = new LazyTiny<LayoutVariable>(() => new ContentWidthVariable(this), LazyThreadSafetyMode.PublicationOnly);
            contentLayoutVariables[(int)LayoutProperty.Height] = new LazyTiny<LayoutVariable>(() => new ContentHeightVariable(this), LazyThreadSafetyMode.PublicationOnly);
            _contentLayoutReferences = contentLayoutVariables;
            _textTopVariableLazy = new LazyTiny<LayoutVariable>(() => new TextTopVariable(this), LazyThreadSafetyMode.PublicationOnly);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable GetContentLayoutReference(LayoutProperty property)
            => _contentLayoutReferences[(int)property].Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(UIElement element) => _children.Add(element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildren(params UIElement[] elements) => _children.AddRange(elements);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChildren(IEnumerable<UIElement> elements) => _children.AddRange(elements);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveChild(UIElement element) => _children.Remove(element);

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            foreach (UIElement child in _children)
                child.ApplyTheme(provider);
            string fontName = provider.FontName;
            _fontName = fontName;
            using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, UIConstants.DefaultFontSize);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            format.TextAlignment = DWriteTextAlignment.Center;
            format.WordWrapping = DWriteWordWrapping.NoWrap;
            Interlocked.Exchange(ref _titleHeight, GraphicsUtils.MeasureTextHeightAsInt("Ty", format));
            DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
            DisposeHelper.SwapDisposeInterlocked(ref _textLayout);
            Update(RenderObjectUpdateFlags.Format, RedrawType.RedrawAllContent);
        }

        protected override void OnThemePrefixChanged(string prefix)
            => UIElementHelper.CopyStringArrayAndAppendDottedPrefix(BrushNamesTemplate, _brushNames, (int)Brush._Last, prefix);

        public void RenderChildBackground(UIElement child, D2D1DeviceContext context)
            => RenderBackground(context, _brushes[(int)Brush.BackBrush]);

        private void Children_BeforeAdded(object? sender, BeforeListAddOrRemoveEventArgs<UIElement> e)
        {
            UIElement child = e.Item;
            if (child.Parent is null)
            {
                child.Parent = this;
                return;
            }
            e.Cancel = true;
            throw new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Update() => Update(RedrawType.RedrawAllContent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update(RedrawType type)
        {
            if (type == RedrawType.NoRedraw)
                return;
            InterlockedHelper.Or(ref _redrawTypeRaw, (long)type);
            UpdateCore();
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags, RedrawType redrawType)
        {
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update(redrawType);
        }

        [Inline(InlineBehavior.Remove)]
        private RedrawType GetRedrawTypeAndReset()
            => (RedrawType)Interlocked.Exchange(ref _redrawTypeRaw, (long)RedrawType.NoRedraw);

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
            => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

        public override bool NeedRefresh()
        {
            if (_redrawTypeRaw > (long)RedrawType.NoRedraw)
                return true;
            return Interlocked.Read(ref _redrawTypeRaw) > (long)RedrawType.NoRedraw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetLayouts(RenderObjectUpdateFlags flags, out DWriteTextLayout? titleLayout, out DWriteTextLayout? textLayout)
        {
            titleLayout = Interlocked.Exchange(ref _titleLayout, null);
            textLayout = Interlocked.Exchange(ref _textLayout, null);

            DWriteFactory factory = SharedResources.DWriteFactory;
            if ((flags & RenderObjectUpdateFlags.Title) == RenderObjectUpdateFlags.Title)
            {
                DWriteTextFormat? format = titleLayout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleCenter, NullSafetyHelper.ThrowIfNull(_fontName), UIConstants.DefaultFontSize);
                titleLayout = factory.CreateTextLayout(_title ?? string.Empty, format);
                format.Dispose();
                titleLayout.MaxWidth = titleLayout.GetMetrics().Width + UIConstants.ElementMarginDouble;
                titleLayout.MaxHeight = InterlockedHelper.Read(ref _titleHeight);
            }
            if ((flags & RenderObjectUpdateFlags.Text) == RenderObjectUpdateFlags.Text)
            {
                DWriteTextFormat? format = textLayout;
                if (CheckFormatIsNotAvailable(format, flags))
                {
                    format = TextFormatHelper.CreateTextFormat(TextAlignment.TopLeft, NullSafetyHelper.ThrowIfNull(_fontName), UIConstants.DefaultFontSize);
                    format.SetLineSpacing(DWriteLineSpacingMethod.Uniform, 20, 16);
                    format.WordWrapping = DWriteWordWrapping.EmergencyBreak;
                }
                textLayout = factory.CreateTextLayout(_text ?? string.Empty, format);
                format.Dispose();
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format, RenderObjectUpdateFlags flags)
        {
            if (format is null || format.IsDisposed)
                return true;
            if ((flags & RenderObjectUpdateFlags.Format) == RenderObjectUpdateFlags.Format)
            {
                format.Dispose();
                return true;
            }
            return false;
        }

        public override void Render(DirtyAreaCollector collector) => Render(collector, markDirty: false);

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            IRenderer renderer = Renderer;
            RedrawType redrawType = GetRedrawTypeAndReset();
            if (collector.IsEmptyInstance) //Force redraw
                redrawType = RedrawType.RedrawAllContent;
            else if (redrawType == RedrawType.NoRedraw)
                return true;
            GetLayouts(GetAndCleanRenderObjectUpdateFlags(), out DWriteTextLayout? titleLayout, out DWriteTextLayout? textLayout);
            D2D1DeviceContext context = renderer.GetDeviceContext();
            D2D1Brush[] brushes = _brushes;
            D2D1Brush backBrush = brushes[(int)Brush.BackBrush];
            D2D1Brush textBrush = brushes[(int)Brush.TextBrush];
            switch (redrawType)
            {
                case RedrawType.RedrawText:
                    RenderText(context, collector, backBrush, textBrush, textLayout, justText: true);
                    break;
                case RedrawType.RedrawAllContent:
                    Rect bounds = Bounds;
                    float lineWidth = renderer.GetBaseLineWidth();
                    RenderBackground(context, backBrush);
                    context.DrawRectangle(GraphicsUtils.AdjustRectangleAsBorderBounds(new Rect(bounds.X, bounds.Y + MathI.Floor(_titleHeight * 0.5f),
                        bounds.Right, bounds.Bottom), lineWidth), brushes[(int)Brush.BorderBrush], lineWidth);
                    RenderTitle(context, backBrush, textBrush, titleLayout, bounds);
                    RenderText(context, collector, backBrush, textBrush, textLayout, justText: false);
                    collector.MarkAsDirty(bounds);
                    break;
            }
            if (titleLayout is not null)
                DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
            if (textLayout is not null)
                DisposeHelper.NullSwapOrDispose(ref _textLayout, textLayout);
            return true;
        }

        [Inline(InlineBehavior.Remove)]
        private void RenderTitle(D2D1DeviceContext context, D2D1Brush backBrush, D2D1Brush textBrush, DWriteTextLayout? layout, in Rect bounds)
        {
            if (layout is null)
                return;

            PointF titleLoc = new PointF(bounds.X + UIConstants.ElementMargin, bounds.Y);
            context.PushAxisAlignedClip(RectF.FromXYWH(titleLoc.X, titleLoc.Y, layout.MaxWidth, layout.MaxHeight), D2D1AntialiasMode.Aliased);
            RenderBackground(context, backBrush);
            context.DrawTextLayout(titleLoc, layout, textBrush, D2D1DrawTextOptions.Clip);
            context.PopAxisAlignedClip();
        }

        [Inline(InlineBehavior.Remove)]
        private void RenderText(D2D1DeviceContext context, DirtyAreaCollector collector, D2D1Brush backBrush, D2D1Brush textBrush,
            DWriteTextLayout? layout, [InlineParameter] bool justText)
        {
            if (layout is null)
                return;
            Rect bounds = Bounds;
            Point textLoc = TextLocation;
            int textBoundRight = bounds.Right - UIConstants.ElementMarginDouble;
            int textBoundBottom = bounds.Bottom - UIConstants.ElementMarginDouble;
            Rect textRect = new Rect(textLoc.X, textLoc.Y, textBoundRight, textBoundBottom);
            if (!textRect.IsValid)
                return;
            context.PushAxisAlignedClip((RectF)textRect, D2D1AntialiasMode.Aliased);
            layout.MaxWidth = textRect.Width;
            if (justText)
            {
                RenderBackground(context, backBrush);
                collector.MarkAsDirty(textRect);
            }
            context.DrawTextLayout(textLoc, layout, textBrush, D2D1DrawTextOptions.None);
            context.PopAxisAlignedClip();
        }

        [Inline(InlineBehavior.Remove)]
        private static int GetContentLeftCore(int x) => x + UIConstants.ElementMarginDouble;

        [Inline(InlineBehavior.Remove)]
        private int GetContentTopCore(int y) => GetTextTopCore(y) + UIConstants.ElementMargin;

        [Inline(InlineBehavior.Remove)]
        private static int GetContentRightCore(int right) => right - UIConstants.ElementMarginDouble;

        [Inline(InlineBehavior.Remove)]
        private static int GetContentBottomCore(int bottom) => bottom - UIConstants.ElementMarginDouble;

        [Inline(InlineBehavior.Remove)]
        private int GetTextTopCore(int y) => y + InterlockedHelper.Read(ref _titleHeight);

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
                DisposeHelper.SwapDisposeInterlocked(ref _textLayout);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
            DisposeChildren(disposing);
        }

        [Inline(InlineBehavior.Remove)]
        private void DisposeChildren(bool disposing)
        {
            IList<UIElement> children = _children.GetUnderlyingList();
            if (disposing)
            {
                int count = children.Count;
                if (children is UnwrappableList<UIElement> castedList)
                {
                    UIElement[] childrenArray = castedList.Unwrap();
                    for (int i = 0; i < count; i++)
                        (childrenArray[i] as IDisposable)?.Dispose();
                }
                else
                {
                    foreach (UIElement child in children)
                        (child as IDisposable)?.Dispose();
                }
            }
            children.Clear();
        }

        ~GroupBox()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
