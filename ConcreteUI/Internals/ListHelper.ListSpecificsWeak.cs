using System;
using System.Collections.Generic;

namespace ConcreteUI.Internals
{
    partial class ListHelper
    {
        private static class ListSpecificsWeak<T>
        {
            public static void CleanAll<TList>(TList list, bool disposing) where TList : IList<T>
            {
                if (disposing)
                {
                    for (int i = 0, count = list.Count; i < count; i++)
                        (list[i] as IDisposable)?.Dispose();
                }
                list.Clear();
            }
        }

    }
}
