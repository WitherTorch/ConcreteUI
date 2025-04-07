using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;
using WitherTorch.Common.Windows;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// Represents a set of run-time bindable and discoverable properties that allow a
    /// data-driven application to modify the state of a Direct2D effect.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe class D2D1Properties : ComObject
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetPropertyCount = _Start,
            GetPropertyName,
            GetPropertyNameLength,
            GetType,
            GetPropertyIndex,
            SetValueByName,
            SetValue,
            GetValueByName,
            GetValue,
            GetValueSize,
            GetSubProperties,
            _End
        }

        public D2D1Properties() : base() { }

        public D2D1Properties(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Returns the total number of custom properties in this object.
        /// </summary>
        public uint Count => GetPropertyCount();

        [Inline(InlineBehavior.Remove)]
        private uint GetPropertyCount()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPropertyCount);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="GetPropertyName(uint, char*, uint)"/>
        public string GetPropertyName(uint index)
        {
            uint length = GetPropertyNameLength(index);
            if (length == 0)
                return string.Empty;
            if (length > int.MaxValue)
                length = int.MaxValue;
            string result = StringHelper.AllocateRawString(unchecked((int)length));
            fixed (char* ptr = result)
                GetPropertyName(index, ptr, length + 1);
            return result;
        }

        /// <summary>
        /// Retrieves the property name from the given property index.
        /// </summary>
        public void GetPropertyName(uint index, char* buffer, uint bufferLength)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPropertyName);
            int hr = ((delegate*<void*, uint, char*, uint, int>)functionPointer)(nativePointer, index, buffer, bufferLength);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Returns the length of the property name from the given index.
        /// </summary>
        public uint GetPropertyNameLength(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPropertyNameLength);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Retrieves the type of the given property.
        /// </summary>
        public D2D1PropertyType GetPropertyType(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetType);
            return ((delegate*<void*, uint, D2D1PropertyType>)functionPointer)(nativePointer, index);
        }

        /// <inheritdoc cref="GetPropertyIndex(char*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetPropertyIndex(string name)
        {
            fixed (char* ptr = name)
                return GetPropertyIndex(ptr);
        }

        /// <summary>
        /// Retrieves the property index for the given property name.
        /// </summary>
        public uint GetPropertyIndex(char* name)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPropertyIndex);
            return ((delegate*<void*, char*, uint>)functionPointer)(nativePointer, name);
        }

        /// <inheritdoc cref="SetValueByName(char*, D2D1PropertyType, byte*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueByName<T>(string name, in T value) where T : unmanaged
        {
            fixed (char* ptr = name)
                SetValueByName(ptr, value);
        }

        /// <inheritdoc cref="SetValueByName(char*, D2D1PropertyType, byte*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueByName(string name, D2D1PropertyType type, byte* data, uint dataSize)
        {
            fixed (char* ptr = name)
                SetValueByName(ptr, type, data, dataSize);
        }

        /// <inheritdoc cref="SetValueByName(char*, D2D1PropertyType, byte*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueByName<T>(char* name, in T value) where T : unmanaged
            => SetValueByName(name, D2D1PropertyType.Unknown, (byte*)UnsafeHelper.AsPointerIn(in value), unchecked((uint)sizeof(T)));

        /// <summary>
        /// Sets the value of the given property using its name.
        /// </summary>
        public void SetValueByName(char* name, D2D1PropertyType type, byte* data, uint dataSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetValueByName);
            int hr = ((delegate*<void*, char*, D2D1PropertyType, byte*, uint, int>)functionPointer)(nativePointer, name, type, data, dataSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="SetValue(uint, D2D1PropertyType, byte*, uint)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue<T>(uint index, in T value) where T : unmanaged
            => SetValue(index, D2D1PropertyType.Unknown, (byte*)UnsafeHelper.AsPointerIn(in value), unchecked((uint)sizeof(T)));

        /// <summary>
        /// Sets the given value using the property index.
        /// </summary>
        public void SetValue(uint index, D2D1PropertyType type, byte* data, uint dataSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetValue);
            int hr = ((delegate*<void*, uint, D2D1PropertyType, byte*, uint, int>)functionPointer)(nativePointer, index, type, data, dataSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="GetValueByName(char*, D2D1PropertyType, byte*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueByName<T>(string name) where T : unmanaged
        {
            fixed (char* ptr = name)
                return GetValueByName<T>(ptr);
        }

        /// <inheritdoc cref="GetValueByName(char*, D2D1PropertyType, byte*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetValueByName(string name, D2D1PropertyType type, byte* data, uint dataSize)
        {
            fixed (char* ptr = name)
                GetValueByName(ptr, type, data, dataSize);
        }

        /// <inheritdoc cref="GetValueByName(char*, D2D1PropertyType, byte*, uint)"/>
        [LocalsInit(false)]
        [Inline(InlineBehavior.Keep, export: true)]
        public T GetValueByName<T>(char* name) where T : unmanaged
        {
            T result;
            GetValueByName(name, D2D1PropertyType.Unknown, (byte*)&result, unchecked((uint)sizeof(T)));
            return result;
        }

        /// <summary>
        /// Retrieves the given property or sub-property by name. '.' is the delimiter for
        /// sub-properties.
        /// </summary>
        public void GetValueByName(char* name, D2D1PropertyType type, byte* data, uint dataSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetValueByName);
            int hr = ((delegate*<void*, char*, D2D1PropertyType, byte*, uint, int>)functionPointer)(nativePointer, name, type, data, dataSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="GetValue(uint, D2D1PropertyType, byte*, uint)"/>
        [LocalsInit(false)]
        public T GetValue<T>(uint index) where T : unmanaged
        {
            T result;
            GetValue(index, D2D1PropertyType.Unknown, (byte*)&result, unchecked((uint)sizeof(T)));
            return result;
        }

        /// <summary>
        /// Retrieves the given value by index.
        /// </summary>
        public void GetValue(uint index, D2D1PropertyType type, byte* data, uint dataSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetValue);
            int hr = ((delegate*<void*, uint, D2D1PropertyType, byte*, uint, int>)functionPointer)(nativePointer, index, type, data, dataSize);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Returns the value size for the given property index.
        /// </summary>
        public uint GetValueSize(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetValueSize);
            return ((delegate*<void*, uint, uint>)functionPointer)(nativePointer, index);
        }

        /// <summary>
        /// Retrieves the sub-properties of the given property by index.
        /// </summary>
        public D2D1Properties GetSubProperties(uint index)
        {
            void* pProperties;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSubProperties);
            int hr = ((delegate*<void*, uint, void**, int>)functionPointer)(nativePointer, index, &pProperties);
            if (hr >= 0)
                return pProperties == null ? null : new D2D1Properties(pProperties, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}