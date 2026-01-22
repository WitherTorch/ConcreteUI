using System;
using System.Collections.Generic;

namespace ConcreteUI.Internals
{
    partial class ListHelper
    {
        private static class ListSpecifics<T> where T : IDisposable
        {
            public static void CleanAll<TList>(TList list, bool disposing) where TList : IList<T>
            {
                if (disposing)
                {
                    for (int i = 0, count = list.Count; i < count; i++)
                        list[i]?.Dispose();
                }
                list.Clear();
            }
        }

    }
}
