using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

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
            return ((delegate* unmanaged[Stdcall]<void*, uint>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private DWriteFontFamily GetFontFamily(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFontFamily);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, int>)functionPointer)(nativePointer, index, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DWriteFontFamily(nativePointer, ReferenceType.Owned);
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
            int hr = ((delegate* unmanaged[Stdcall]<void*, char*, uint*, bool*, int>)functionPointer)(nativePointer, familyName, index, &exists);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return exists;
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

            public DWriteFontFamily Current
            {
                get
                {
                    uint index = _index;
                    if (index >= _bound)
                        throw new InvalidOperationException();
                    return _collection[index];
                }
            }

            object IEnumerator.Current => Current;

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