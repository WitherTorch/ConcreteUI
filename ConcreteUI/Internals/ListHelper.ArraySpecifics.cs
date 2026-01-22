using System;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Internals
{
    partial class ListHelper
    {
        private static class ArraySpecifics<T> where T : IDisposable
        {
            public static void CleanAll(T?[] array, int length, bool disposing)
            {
                if (length <= 0)
                    return;
                CleanAllCore(ref array[0], (nuint)length, disposing);
            }

            private static void CleanAllCore(ref T? arrayRef, nuint length, bool disposing)
            {
                if (disposing)
                {
                    for (nuint i = 0; i < length; i++)
                        UnsafeHelper.AddTypedOffset(ref arrayRef, i)?.Dispose();
                }
                UnsafeHelper.InitBlock(ref UnsafeHelper.As<T?, byte>(ref arrayRef), 0, length * UnsafeHelper.SizeOf<T>());
            }

            public static void DisposeAll(T?[] array, int length)
            {
                if (length <= 0)
                    return;
                DisposeAllCore(ref array[0], (nuint)length);
            }

            public static void DisposeAll_Unsafe(T?[] array, nuint length)
              => DisposeAllCore(ref array[0], length);

            private static void DisposeAllCore(ref T? arrayRef, nuint length)
            {
                for (nuint i = 0; i < length; i++)
                    UnsafeHelper.AddTypedOffset(ref arrayRef, i)?.Dispose();
            }
        }
    }
}
