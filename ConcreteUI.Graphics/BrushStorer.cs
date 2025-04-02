using System;
using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Graphics
{
    public sealed class BrushStorer : IDisposable
    {
        private static readonly BrushStorer _instance = new BrushStorer();

        public static BrushStorer Instance => _instance;

        private BrushStorer() { }

        private readonly Dictionary<string, D2D1Brush> _brushDictionary = new Dictionary<string, D2D1Brush>();
        private readonly List<IBrushStyler> _brushStylerList = new List<IBrushStyler>();

        private bool _disposed;

        public void AddStyler(IBrushStyler styler)
        {
            _brushStylerList.Add(styler);
        }

        public void SaveBrush(string key, D2D1Brush value, bool overrides = false)
        {
            string realKey = key.ToLower();
            if (_brushDictionary.TryGetValue(realKey, out D2D1Brush oldBrush))
            {
                if (oldBrush != value && (overrides || oldBrush is null || oldBrush.IsDisposed))
                {
                    _brushDictionary[realKey] = value;
                    int count = _brushStylerList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        IBrushStyler styler = _brushStylerList[i];
                        string prefix = styler.GetPrefix();
                        if (realKey.StartsWith(styler.GetPrefix(), StringComparison.OrdinalIgnoreCase))
                        {
                            styler.SetBrush(realKey.Substring(prefix.Length).TrimStart('.'), value);
                        }
                    }
                }
            }
            else
            {
                _brushDictionary.Add(realKey, value);
                int count = _brushStylerList.Count;
                for (int i = 0; i < count; i++)
                {
                    IBrushStyler styler = _brushStylerList[i];
                    string prefix = styler.GetPrefix();
                    if (realKey.StartsWith(styler.GetPrefix(), StringComparison.OrdinalIgnoreCase))
                    {
                        styler.SetBrush(realKey.Substring(prefix.Length).TrimStart('.'), value);
                    }
                }
            }
        }

        public void RemoveBrush(string key)
        {
            string realKey = key.ToLower();
            if (_brushDictionary.TryGetValue(realKey, out D2D1Brush value))
            {
                var Brush = value;
                _brushDictionary.Remove(realKey);
            }
        }

        public bool Contains(string key)
        {
            return _brushDictionary.ContainsKey(key.ToLower());
        }

        public D2D1Brush this[string key]
        {
            get
            {
                return _brushDictionary[key.ToLower()];
            }
            set
            {
                SaveBrush(key.ToLower(), value);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                    _brushDictionary.Clear();
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                _disposed = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        ~BrushStorer()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
