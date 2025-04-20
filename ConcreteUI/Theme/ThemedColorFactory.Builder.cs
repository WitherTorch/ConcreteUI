using System;
using System.Collections;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;

using WitherTorch.Common.Collections;

namespace ConcreteUI.Theme
{
    partial class ThemedColorFactory
    {
        public sealed class Builder : ICloneable
        {
            private readonly UnwrappableList<byte> _variantKeyList;
            private readonly UnwrappableList<D2D1ColorF> _variantColorList;
            private D2D1ColorF _base;

            public Builder()
            {
                _base = default;
                _variantKeyList = new UnwrappableList<byte>(capacity: 0);
                _variantColorList = new UnwrappableList<D2D1ColorF>(capacity: 0);
            }

            internal Builder(in D2D1ColorF baseColor) : this()
            {
                _base = baseColor;
            }

            internal Builder(in D2D1ColorF baseColor, byte[] variantKeys, D2D1ColorF[] variantColors)
            {
                int length = variantKeys.Length;
                if (length != variantColors.Length)
                    throw new InvalidOperationException();
                _base = baseColor;
                _variantKeyList = new UnwrappableList<byte>(variantKeys);
                _variantColorList = new UnwrappableList<D2D1ColorF>(variantColors);
            }

            private Builder(Builder original)
            {
                _base = original._base;
                _variantKeyList = new UnwrappableList<byte>(original._variantKeyList);
                _variantColorList = new UnwrappableList<D2D1ColorF>(original._variantColorList);
            }

            public D2D1ColorF this[WindowMaterial material]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    int index = _variantKeyList.IndexOf((byte)material);
                    if (index < 0)
                        return _base;
                    return _variantColorList[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder WithDefault(in D2D1ColorF color)
            {
                _base = color;
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder WithVariant(WindowMaterial material, IThemedColorFactory factory)
                => WithVariant(material, factory.CreateColorByMaterial(material));

            public Builder WithVariant(WindowMaterial material, in D2D1ColorF color)
            {
                UnwrappableList<byte> variantKeyList = _variantKeyList;
                UnwrappableList<D2D1ColorF> variantColorList = _variantColorList;
                byte key = (byte)material;
                int index = variantKeyList.IndexOf(key);
                if (index >= 0)
                {
                    variantColorList[index] = color;
                    return this;
                }
                variantKeyList.Add(key);
                variantColorList.Add(color);
                return this;
            }

            public Builder Clone() => new Builder(this);

            public IThemedColorFactory Build()
            {
                if (_variantKeyList.Count > 0)
                    return new ThemedColorFactoryImpl(_base, _variantKeyList.ToArray(), _variantColorList.ToArray());
                return new SimpleThemedColorFactoryImpl(_base);
            }

            object ICloneable.Clone() => Clone();
        }
    }
}
