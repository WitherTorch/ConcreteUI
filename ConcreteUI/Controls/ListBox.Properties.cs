using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using WitherTorch.Common;
using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class ListBox
    {
        public event EventHandler? SelectedIndicesChanged;

        public string[] SelectedItems
        {
            get
            {
                ObservableList<string> items = _items;
                int count = items.Count;
                if (count <= 0)
                    return Array.Empty<string>();
                ArrayPool<string> pool = ArrayPool<string>.Shared;
                string[] buffer = pool.Rent(count);
                try
                {
                    CopySelectedItemsToBufferCore(items, count, buffer, 0, out int resultLength);
                    if (resultLength <= 0)
                        return Array.Empty<string>();
                    string[] result = new string[resultLength];
                    Array.Copy(buffer, result, resultLength);
                    return result;
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        public int[] SelectedIndices
        {
            get
            {
                ObservableList<string> items = _items;
                int count = items.Count;
                if (count <= 0)
                    return Array.Empty<int>();
                ArrayPool<int> pool = ArrayPool<int>.Shared;
                int[] buffer = pool.Rent(count);
                try
                {
                    CopySelectedIndicesToBufferCore(count, buffer, 0, out int resultLength);
                    if (resultLength <= 0)
                        return Array.Empty<int>();
                    int[] result = new int[resultLength];
                    Array.Copy(buffer, result, resultLength);
                    return result;
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
        }

        public int ItemHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MathI.Ceiling(_itemHeight);
        }

        public ListBoxMode Mode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _chooseMode;
            set
            {
                if (_chooseMode == value)
                    return;
                _chooseMode = value;
                Update();
            }
        }

        public IList<string> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _items;
        }

        public float FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fontSize;
            set
            {
                if (_fontSize == value)
                    return;
                _fontSize = value;
                DisposeHelper.SwapDisposeInterlocked(ref _format);
                Interlocked.Exchange(ref _recalcFormat, Booleans.TrueLong);
                if (Items.Count > 0)
                    Update();
            }
        }

        public string CheckBoxThemePrefix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _checkBoxThemePrefix;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (SequenceHelper.Equals(_checkBoxThemePrefix, value))
                    return;
                _checkBoxThemePrefix = value.ToLowerAscii();
            }
        }
    }
}
