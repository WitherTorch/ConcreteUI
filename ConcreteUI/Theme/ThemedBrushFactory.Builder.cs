﻿using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

using WitherTorch.Common.Collections;

namespace ConcreteUI.Theme
{
    partial class ThemedBrushFactory
    {
        public sealed class Builder : ICloneable
        {
            private readonly UnwrappableList<byte> _variantKeyList;
            private readonly UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>> _variantBrushFactoryList;

            private Func<D2D1DeviceContext, D2D1Brush> _base;

            internal Builder(Func<D2D1DeviceContext, D2D1Brush> baseBrushFactory)
            {
                _base = baseBrushFactory;
                _variantKeyList = new UnwrappableList<byte>(capacity: 0);
                _variantBrushFactoryList = new UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>>(capacity: 0);
            }

            internal Builder(Func<D2D1DeviceContext, D2D1Brush> baseBrushFactory, byte[] variantKeys, Func<D2D1DeviceContext, D2D1Brush>[] variantBrushes)
            {
                int length = variantKeys.Length;
                if (length != variantBrushes.Length)
                    throw new InvalidOperationException();
                _base = baseBrushFactory;
                _variantKeyList = new UnwrappableList<byte>(variantKeys);
                _variantBrushFactoryList = new UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>>(variantBrushes);
            }

            private Builder(Builder original)
            {
                _base = original._base;
                _variantKeyList = new UnwrappableList<byte>(original._variantKeyList);
                _variantBrushFactoryList = new UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>>(original._variantBrushFactoryList);
            }

            public Func<D2D1DeviceContext, D2D1Brush> this[WindowMaterial material]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    int index = _variantKeyList.IndexOf((byte)material);
                    if (index < 0)
                        return _base;
                    return _variantBrushFactoryList[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder WithVariant(WindowMaterial material, in D2D1ColorF brushColor)
                => WithVariant(material, CreateFactoryByColor(brushColor));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder WithVariant(WindowMaterial material, IThemedBrushFactory brushFactory)
                => WithVariant(material, context => brushFactory.CreateBrushByMaterial(context, material));

            public Builder WithVariant(WindowMaterial material, Func<D2D1DeviceContext, D2D1Brush> brushFactory)
            {
                if (material < WindowMaterial.None || material >= WindowMaterial._Last)
                    throw new ArgumentOutOfRangeException(nameof(material));
                UnwrappableList<byte> variantKeyList = _variantKeyList;
                UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>> variantBrushFactoryList = _variantBrushFactoryList;
                byte key = (byte)material;
                int index = variantKeyList.IndexOf(key);
                if (index >= 0)
                {
                    variantBrushFactoryList[index] = brushFactory;
                    return this;
                }
                variantKeyList.Add(key);
                variantBrushFactoryList.Add(brushFactory);
                return this;
            }

            public Builder Clone() => new Builder(this);

            public IThemedBrushFactory Build()
            {
                if (_variantKeyList.Count > 0)
                    return new ThemedBrushFactoryImpl(_base, _variantKeyList.ToArray(), _variantBrushFactoryList.ToArray());
                return new SimpleThemedBrushFactoryImpl(_base);
            }

            object ICloneable.Clone() => Clone();
        }
    }
}
