using System;

using ConcreteUI.Graphics.Native.DirectWrite;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        public struct SelectionRange
        {
            private int _startPos, _endPos, _length;
            private DWriteTextRange cachedRange;
            private bool cached;

            public SelectionRange(int startPos, int endPos)
            {
                _startPos = startPos;
                _endPos = endPos;
                _length = Math.Abs(_endPos - _startPos);
                cachedRange = default;
                cached = false;
            }


            public int StartPosition
            {
                get => _startPos;
                set
                {
                    if (_startPos != value)
                    {
                        _startPos = value;
                        _length = Math.Abs(_endPos - _startPos);
                        cached = false;
                    }
                }
            }

            public int Length
            {
                get => _length;
                set
                {
                    if (_length != value)
                    {
                        _length = Math.Abs(value);
                        _endPos = _startPos + value;
                        cached = false;
                    }
                }
            }

            public int EndPosition
            {
                get => _endPos;
                set
                {
                    if (_endPos != value)
                    {
                        _endPos = value;
                        _length = Math.Abs(_endPos - _startPos);
                        cached = false;
                    }
                }
            }

            public DWriteTextRange ToTextRange()
            {
                if (!cached)
                {
                    cachedRange.StartPosition = MathHelper.Min(_startPos, _endPos);
                    cachedRange.Length = _length;
                    cached = true;
                }
                return cachedRange;
            }
        }
    }
}
