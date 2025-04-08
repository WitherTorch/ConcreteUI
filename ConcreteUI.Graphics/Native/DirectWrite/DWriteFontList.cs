using ConcreteUI.Graphics.Extensions;

using InlineMethod;

using LocalsInit;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The <see cref="DWriteFontList"/> class represents an ordered set of fonts that are part of a <see cref="DWriteFontCollection"/>
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DWriteFontList : ComObject, IReadOnlyList<DWriteFont>
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetFontCollection = _Start,
            GetFontCount,
            GetFont,
            _End,
        }

        public DWriteFontList() : base() { }

        public DWriteFontList(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets the number of fonts in the font list.
        /// </summary>
        public uint Count => GetFontCount();

        int IReadOnlyCollection<DWriteFont>.Count => MathHelper.MakeSigned(GetFontCount());

        /// <inheritdoc cref="this[uint]"/>
        public DWriteFont this[int index]
        {
            [LocalsInit(false)]
            get => index < 0 ?
                throw new ArgumentOutOfRangeException(nameof(index)) :
                GetFont(unchecked((uint)index));
        }

        /// <summary>
        /// Gets a font given its zero-based index.
        /// </summary>
        /// <param name="index">Zero-based index of the font family.</param>
        /// <returns>
        /// The newly created font object.
        /// </returns>
        public DWriteFont this[uint index]
        {
            [LocalsInit(false)]
            get => GetFont(index);
        }

        public IEnumerator<DWriteFont> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public DWriteFontCollection GetFontCollection()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontCollection);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DWriteFontCollection(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Keep)]
        private uint GetFontCount()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontCount);
            return ((delegate* unmanaged[Stdcall]<void*, uint>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Keep)]
        private DWriteFont GetFont(uint index)
        {
            void* font;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFont);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, int>)functionPointer)(nativePointer, index, &font);
            if (hr >= 0)
                return font == null ? null : new DWriteFont(font, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        private sealed class Enumerator : IEnumerator<DWriteFont>
        {
            private readonly DWriteFontList _list;
            private readonly uint _bound;

            private uint _index;

            public Enumerator(DWriteFontList list)
            {
                _list = list;
                _bound = list.Count - 1;
                _index = uint.MaxValue;
            }

            public DWriteFont Current => _index < 0 || _index >= _bound ? default : _list[_index];

            object IEnumerator.Current => _index < 0 || _index >= _bound ? default : _list[_index];

            public bool MoveNext()
            {
                uint index = _index;
                if (index == uint.MaxValue)
                {
                    _index = 0;
                    return true;
                }
                if (index < _bound)
                {
                    _index = index + 1;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _index = uint.MaxValue;
            }

            public void Dispose()
            {
                Reset();
                GC.SuppressFinalize(this);
            }
        }
    }
}