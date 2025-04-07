using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics.Extensions;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The <see cref="DWriteFontCollection"/> encapsulates a collection of font families.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DWriteFontCollection : ComObject, IReadOnlyCollection<DWriteFontFamily>
    {
        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetFontFamilyCount = _Start,
            GetFontFamily,
            FindFamilyName,
            GetFontFromFontFace,
            _End,
        }

        public DWriteFontCollection() : base() { }

        public DWriteFontCollection(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets the number of font families in the collection.
        /// </summary>
        public uint Count => GetFontFamilyCount();

        int IReadOnlyCollection<DWriteFontFamily>.Count => MathHelper.MakeSigned(GetFontFamilyCount());

        /// <inheritdoc cref="this[uint]"/>
        public DWriteFontFamily this[int index]
        {
            [LocalsInit(false)]
            get => index < 0 ?
                throw new ArgumentOutOfRangeException(nameof(index)) :
                GetFontFamily(unchecked((uint)index));
        }

        /// <summary>
        /// Creates a font family object given a zero-based font family index.
        /// </summary>
        /// <param name="index">Zero-based index of the font family.</param>
        /// <returns>
        /// The newly created font family object.
        /// </returns>
        public DWriteFontFamily this[uint index]
        {
            [LocalsInit(false)]
            get => GetFontFamily(index);
        }

        public IEnumerator<DWriteFontFamily> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        [Inline(InlineBehavior.Remove)]
        private uint GetFontFamilyCount()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamilyCount);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontFamily GetFontFamily(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamily);
            int hr = ((delegate*<void*, uint, void**, int>)functionPointer)(nativePointer, index, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DWriteFontFamily(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="FindFamilyName(char*, uint*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindFamilyName(string familyName, out uint index)
        {
            fixed (char* ptr = familyName)
                return FindFamilyName(ptr, out index);
        }

        /// <inheritdoc cref="FindFamilyName(char*, uint*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindFamilyName(char* familyName, out uint index)
            => FindFamilyName(familyName, UnsafeHelper.AsPointerOut(out index));

        /// <summary>
        /// Finds the font family with the specified family name.
        /// </summary>
        /// <param name="familyName">Name of the font family. The name is not case-sensitive but must otherwise exactly match a family name in the collection.</param>
        /// <param name="index">Receives the zero-based index of the matching font family if the family name was found or UINT_MAX otherwise.</param>
        /// <returns>
        /// <see langword="true"/> if the family name exists or <see langword="false"/> otherwise.
        /// </returns>
        [LocalsInit(false)]
        public bool FindFamilyName(char* familyName, uint* index)
        {
            bool exists;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FindFamilyName);
            int hr = ((delegate*<void*, char*, uint*, bool*, int>)functionPointer)(nativePointer, familyName, index, &exists);
            if (hr >= 0)
                return exists;
            throw Marshal.GetExceptionForHR(hr);
        }

        private sealed class Enumerator : IEnumerator<DWriteFontFamily>
        {
            private readonly DWriteFontCollection _collection;
            private readonly uint _bound;

            private uint _index;

            public Enumerator(DWriteFontCollection collection)
            {
                _collection = collection;
                _bound = collection.Count - 1;
                _index = uint.MaxValue;
            }

            public DWriteFontFamily Current => _index < 0 || _index >= _bound ? default : _collection[_index];

            object IEnumerator.Current => _index < 0 || _index >= _bound ? default : _collection[_index];

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