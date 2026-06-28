using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;

using ShioUI.Controls;
using ShioUI.Internals;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI.Extensions;

public static class UIElementExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Focus<TElement>(this TElement _this) where TElement : UIElement, IFocusChangedHandler
        => _this.RootWindow.ChangeFocusElement(_this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFocus<TElement>(this TElement _this, bool state) where TElement : UIElement, IFocusChangedHandler
    {
        if (state)
            _this.RootWindow.ChangeFocusElement(_this);
        else
            _this.RootWindow.ClearFocusElement(elementForValidation: _this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point LocalToPage(this UIElement _this, Point point)
        => GraphicsUtils.PointToPage(_this.Location, point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF LocalToPage(this UIElement _this, PointF point)
        => GraphicsUtils.PointToPage(_this.Location, point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point PageToLocal(this UIElement _this, Point point)
        => GraphicsUtils.PointToLocal(_this.Location, point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF PageToLocal(this UIElement _this, PointF point)
        => GraphicsUtils.PointToLocal(_this.Location, point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point PageToWindow(this UIElement _this, Point point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ShioSettings.UseDebugMode)
        {
            using PooledList<UIElement> list = new PooledList<UIElement>();
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element || list.Contains(element))
                    return InvalidOperationException.Throw<Point>();

                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);

                list.Add(element);
                parent = parent.Parent;
            }
        }
        else
        {
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element)
                    return InvalidOperationException.Throw<Point>();
                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);
                parent = parent.Parent;
            }
        }
        return rootWindow.PageToWindow(point);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF PageToWindow(this UIElement _this, PointF point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ShioSettings.UseDebugMode)
        {
            using PooledList<UIElement> list = new PooledList<UIElement>();
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element || list.Contains(element))
                    return InvalidOperationException.Throw<Point>();

                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);

                list.Add(element);
                parent = parent.Parent;
            }
        }
        else
        {
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element)
                    return InvalidOperationException.Throw<Point>();
                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);
                parent = parent.Parent;
            }
        }
        return rootWindow.PageToWindow(point);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point WindowToPage(this UIElement _this, Point point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ReferenceEquals(parent, rootWindow))
            return rootWindow.WindowToPage(point);
        if (parent is not UIElement parentElement)
            return InvalidOperationException.Throw<Point>();
        using ArrayPool<UIElement>.RentScope scope = UIElementHelperInternal.GetPathOfParentChain(rootWindow, parentElement);
        ref readonly UIElement scopeRef = ref scope.GetReferenceOfFirstElement();
        point = rootWindow.WindowToPage(point);
        for (int i = scope.Count - 1; i >= 0; i--)
        {
            UIElement element = UnsafeHelper.AddTypedOffsetAsReadOnly(in scopeRef, i);
            if (element is IRenderWindow window)
                point = window.PageToInnerPage(point);
            else
                point = element.PageToLocal(point);
        }
        return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF WindowToPage(this UIElement _this, PointF point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ReferenceEquals(parent, rootWindow))
            return rootWindow.WindowToPage(point);
        if (parent is not UIElement parentElement)
            return InvalidOperationException.Throw<PointF>();
        using ArrayPool<UIElement>.RentScope scope = UIElementHelperInternal.GetPathOfParentChain(rootWindow, parentElement);
        ref readonly UIElement scopeRef = ref scope.GetReferenceOfFirstElement();
        point = rootWindow.WindowToPage(point);
        for (int i = scope.Count - 1; i >= 0; i--)
        {
            UIElement element = UnsafeHelper.AddTypedOffsetAsReadOnly(in scopeRef, i);
            if (element is IRenderWindow window)
                point = window.PageToInnerPage(point);
            else
                point = element.PageToLocal(point);
        }
        return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point LocalPageToGlobalPage(this UIElement _this, Point point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ShioSettings.UseDebugMode)
        {
            using PooledList<UIElement> list = new PooledList<UIElement>();
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element || list.Contains(element))
                    return InvalidOperationException.Throw<Point>();

                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);

                list.Add(element);
                parent = parent.Parent;
            }
        }
        else
        {
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element)
                    return InvalidOperationException.Throw<Point>();
                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);
                parent = parent.Parent;
            }
        }
        return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF LocalPageToGlobalPage(this UIElement _this, PointF point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ShioSettings.UseDebugMode)
        {
            using PooledList<UIElement> list = new PooledList<UIElement>();
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element || list.Contains(element))
                    return InvalidOperationException.Throw<Point>();

                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);

                list.Add(element);
                parent = parent.Parent;
            }
        }
        else
        {
            while (!ReferenceEquals(parent, rootWindow))
            {
                if (parent is not UIElement element)
                    return InvalidOperationException.Throw<Point>();
                if (parent is IRenderWindow window)
                    point = window.InnerPageToPage(point);
                else
                    point = element.LocalToPage(point);
                parent = parent.Parent;
            }
        }
        return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point GlobalPageToLocalPage(this UIElement _this, Point point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ReferenceEquals(parent, rootWindow))
            return point;
        if (parent is not UIElement parentElement)
            return InvalidOperationException.Throw<Point>();
        using ArrayPool<UIElement>.RentScope scope = UIElementHelperInternal.GetPathOfParentChain(rootWindow, parentElement);
        ref readonly UIElement scopeRef = ref scope.GetReferenceOfFirstElement();
        for (int i = scope.Count - 1; i >= 0; i--)
        {
            UIElement element = UnsafeHelper.AddTypedOffsetAsReadOnly(in scopeRef, i);
            if (element is IRenderWindow window)
                point = window.PageToInnerPage(point);
            else
                point = element.PageToLocal(point);
        }
        return point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF GlobalPageToLocalPage(this UIElement _this, PointF point)
    {
        IElementContainer parent = _this.Parent;
        CoreWindow rootWindow = parent.RootWindow;
        if (ReferenceEquals(parent, rootWindow))
            return point;
        if (parent is not UIElement parentElement)
            return InvalidOperationException.Throw<PointF>();
        using ArrayPool<UIElement>.RentScope scope = UIElementHelperInternal.GetPathOfParentChain(rootWindow, parentElement);
        ref readonly UIElement scopeRef = ref scope.GetReferenceOfFirstElement();
        for (int i = scope.Count - 1; i >= 0; i--)
        {
            UIElement element = UnsafeHelper.AddTypedOffsetAsReadOnly(in scopeRef, i);
            if (element is IRenderWindow window)
                point = window.PageToInnerPage(point);
            else
                point = element.PageToLocal(point);
        }
        return point;
    }
}