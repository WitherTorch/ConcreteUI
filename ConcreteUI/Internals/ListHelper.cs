using System;
using System.Collections.Generic;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Internals
{
    internal static partial class ListHelper
    {
        public static void CleanAll<T, TList>(TList list, bool disposing) where TList : IList<T> where T : IDisposable
        {
            if (typeof(TList) == typeof(T[]))
                goto Array;
            if (typeof(TList) == typeof(UnwrappableList<T>))
                goto UnwrappableList;
            if (typeof(TList) == typeof(ObservableList<T>))
                goto ObservableList;

            Top: switch (list)
            {
                case T[]:
                    goto Array;
                case UnwrappableList<T>:
                    goto UnwrappableList;
                case ObservableList<T>:
                    goto ObservableList;
                default:
                    goto Other;
            }

        Array:
            T[] array = UnsafeHelper.As<TList, T[]>(list);
            ArraySpecifics<T>.CleanAll(array, array.Length, disposing);
            return;

        UnwrappableList:
            ArraySpecifics<T>.CleanAll(UnsafeHelper.As<TList, UnwrappableList<T>>(list).Unwrap(), list.Count, disposing);
            return;

        ObservableList:
            list = UnsafeHelper.As<IList<T>, TList>(UnsafeHelper.As<TList, ObservableList<T>>(list).GetUnderlyingList());
            goto Top;

        Other:
            ListSpecifics<T>.CleanAll(list, disposing);
            return;
        }

        public static void CleanAllWeak<T, TList>(TList list, bool disposing) where TList : IList<T>
        {
            if (typeof(TList) == typeof(T[]))
                goto Array;
            if (typeof(TList) == typeof(UnwrappableList<T>))
                goto UnwrappableList;
            if (typeof(TList) == typeof(ObservableList<T>))
                goto ObservableList;

            Top: switch (list)
            {
                case T[]:
                    goto Array;
                case UnwrappableList<T>:
                    goto UnwrappableList;
                case ObservableList<T>:
                    goto ObservableList;
                default:
                    goto Other;
            }

        Array:
            T[] array = UnsafeHelper.As<TList, T[]>(list);
            ArraySpecificsWeak<T>.CleanAll(array, array.Length, disposing);
            return;

        UnwrappableList:
            int count = list.Count;
            if (count <= 0)
                return;
            if (disposing)
                ArraySpecificsWeak<T>.DisposeAll_Unsafe(UnsafeHelper.As<TList, UnwrappableList<T>>(list).Unwrap(), (nuint)count);
            list.Clear();
            return;

        ObservableList:
            list = UnsafeHelper.As<IList<T>, TList>(UnsafeHelper.As<TList, ObservableList<T>>(list).GetUnderlyingList());
            goto Top;

        Other:
            ListSpecificsWeak<T>.CleanAll(list, disposing);
            return;
        }
    }
}
