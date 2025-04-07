using System.Runtime.InteropServices;

using InlineMethod;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Effects
{
    public unsafe class D2D1Effect : D2D1Properties
    {
        protected new enum MethodTable
        {
            _Start = D2D1Properties.MethodTable._End,
            SetInput,
            SetInputCount,
            GetInput,
            GetInputCount,
            GetOutput,
            _End
        }

        public D2D1Effect() : base() { }

        public D2D1Effect(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public uint InputCount
        {
            get => GetInputCount();
            set => SetInputCount(value);
        }

        public void SetInput(uint index, D2D1Image input, bool invalidate = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetInput);
            ((delegate*<void*, uint, void*, bool, void>)functionPointer)(nativePointer, index, input == null ? null : input.NativePointer, invalidate);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetInputCount(uint inputCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetInputCount);
            int hr = ((delegate*<void*, uint, int>)functionPointer)(nativePointer, inputCount);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        public D2D1Image GetInput(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetInput);
            ((delegate*<void*, uint, void*, void>)functionPointer)(nativePointer, index, &nativePointer);
            return nativePointer == null ? null : new D2D1Image(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Remove)]
        private uint GetInputCount()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetInputCount);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        public D2D1Image GetOutput()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetOutput);
            ((delegate*<void*, void*, void>)functionPointer)(nativePointer, &nativePointer);
            return nativePointer == null ? null : new D2D1Image(nativePointer, ReferenceType.Owned);
        }
    }
}