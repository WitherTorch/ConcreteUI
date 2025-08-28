using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

using ConcreteUI.Controls;
using ConcreteUI.Input;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Window;
using ConcreteUI.Window2;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Test
{
    internal sealed class MainWindow : TabbedWindow
    {
        private readonly UnwrappableList<UIElement>[] _elementLists = new UnwrappableList<UIElement>[3];
        private InputMethod? _ime;
        private ProgressBar? _progressBar;

        public MainWindow(CoreWindow? parent) : base(parent, ["頁面A", "頁面B", "頁面C"])
        {
            InitializeBaseInformation();
        }

        protected override CreateWindowInfo GetCreateWindowInfo()
        {
            CreateWindowInfo windowInfo = base.GetCreateWindowInfo();
            windowInfo.Width = 950;
            windowInfo.Height = 700;
            return windowInfo;
        }

        protected override void OnHandleCreated(nint handle)
        {
            base.OnHandleCreated(handle);
            if (!Screen.TryGetScreenInfoFromHwnd(handle, out ScreenInfo info))
                return;
            Rect bounds = RawBounds;
            Rect screenBounds = info.Bounds;
            RawBounds = new Rectangle(
                x: screenBounds.X + ((screenBounds.Width - bounds.Width) / 2),
                y: screenBounds.Y + ((screenBounds.Height - bounds.Height) / 2),
                height: bounds.Height,
                width: bounds.Width);
        }

        private void InitializeBaseInformation()
        {
            MinimumSize = new Size(640, 560);
            Text = nameof(MainWindow);
            using Stream? stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("ConcreteUI.Test.app-icon.ico");
            if (stream is null)
                return;
            Icon = new Icon(stream);
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            foreach (UnwrappableList<UIElement> elementList in _elementLists)
            {
                if (elementList is null)
                    continue;
                foreach (UIElement element in elementList)
                {
                    if (element is null)
                        continue;
                    element.ApplyTheme(provider);
                }
            }
        }

        protected override IEnumerable<UIElement> GetRenderingElements(int pageIndex)
            => _elementLists[pageIndex];

        protected override void InitializeElements()
        {
            _ime = new InputMethod(this);
            UnwrappableList<UIElement> elementList = new UnwrappableList<UIElement>();

            Button button = new Button(this)
            {
                Text = "請點我!",
                LeftVariable = LayoutVariable.PageReference(LayoutProperty.Left) + UIConstants.ElementMargin,
                TopVariable = LayoutVariable.PageReference(LayoutProperty.Top) + UIConstants.ElementMargin,
            }.WithAutoWidth().WithAutoHeight();
            button.Click += Button_Click;
            elementList.Add(button);

            TextBox textBox = new TextBox(this, _ime)
            {
                LeftVariable = button.RightReference + UIConstants.ElementMargin,
                TopVariable = LayoutVariable.PageReference(LayoutProperty.Top) + UIConstants.ElementMargin,
                RightVariable = LayoutVariable.PageReference(LayoutProperty.Right) - UIConstants.ElementMargin,
                HeightVariable = button.HeightReference,
                Watermark = "這裡可以輸入文字喔!"
            };
            elementList.Add(textBox);

            ListBox listBox = new ListBox(this)
            {
                Mode = ListBoxMode.Some,
                LeftVariable = LayoutVariable.PageReference(LayoutProperty.Left) + UIConstants.ElementMargin,
                TopVariable = textBox.BottomReference + UIConstants.ElementMargin,
                WidthVariable = 250,
                BottomVariable = LayoutVariable.PageReference(LayoutProperty.Bottom) - UIConstants.ElementMargin,
            };
            IList<string> items = listBox.Items;
            for (int i = 1; i <= 200; i++)
                items.Add("物件 " + i.ToString());
            elementList.Add(listBox);

            GroupBox groupBox = new GroupBox(this)
            {
                LeftVariable = listBox.RightReference + UIConstants.ElementMargin,
                TopVariable = textBox.BottomReference + UIConstants.ElementMargin,
                RightVariable = LayoutVariable.PageReference(LayoutProperty.Right) - UIConstants.ElementMargin,
                BottomVariable = LayoutVariable.PageReference(LayoutProperty.Bottom) - UIConstants.ElementMargin,
                Title = "群組容器",
            };
            elementList.Add(groupBox);

            groupBox.AddChild(new CheckBox(this)
            {
                LeftVariable = groupBox.ContentLeftReference,
                TopVariable = groupBox.ContentTopReference,
                Text = "可以勾選的方塊"
            }.WithAutoWidth().WithAutoHeight());

            ComboBox comboBox = new ComboBox(this)
            {
                LeftVariable = groupBox.ContentLeftReference,
                TopVariable = groupBox.FirstChild!.BottomReference + UIConstants.ElementMargin,
                WidthVariable = 200
            }.WithAutoHeight();
            comboBox.RequestDropdownListOpening += ComboBox_RequestDropdownListOpening;
            items = comboBox.Items;
            for (int i = 1; i <= 200; i++)
                items.Add("選項 " + i.ToString());
            groupBox.AddChild(comboBox);
            Label label = new Label(this)
            {
                LeftVariable = groupBox.ContentLeftReference,
                TopVariable = comboBox.BottomReference + UIConstants.ElementMargin,
                RightVariable = groupBox.ContentRightReference,
                Text = "底下是進度條測試",
                Alignment = TextAlignment.MiddleCenter
            }.WithAutoHeight();
            Button leftButton = new Button(this)
            {
                LeftVariable = label.LeftReference,
                TopVariable = label.BottomReference + UIConstants.ElementMargin,
                Text = "-"
            }.WithAutoWidth().WithAutoHeight();
            Button rightButton = new Button(this)
            {
                TopVariable = label.BottomReference + UIConstants.ElementMargin,
                RightVariable = label.RightReference,
                Text = "+"
            }.WithAutoWidth().WithAutoHeight();
            ProgressBar progressBar = new ProgressBar(this)
            {
                LeftVariable = leftButton.RightReference + UIConstants.ElementMargin,
                TopVariable = label.BottomReference + UIConstants.ElementMargin,
                RightVariable = rightButton.LeftReference - UIConstants.ElementMargin,
                HeightVariable = leftButton.HeightReference,
                Maximium = 100.0f,
                Value = 50.0f
            };
            leftButton.Click += LeftButton_Click;
            rightButton.Click += RightButton_Click;
            groupBox.AddChildren(label, leftButton, rightButton, progressBar);
            _progressBar = progressBar;

            _elementLists[0] = elementList;
            _elementLists[1] = new();
            _elementLists[2] = new();
        }

        private void ComboBox_RequestDropdownListOpening(object? sender, DropdownListEventArgs e)
        {
            ChangeOverlayElement(e.DropdownList);
        }

        private void Button_Click(UIElement sender, in MouseInteractEventArgs args)
        {
            if (CurrentTheme?.IsDarkTheme ?? false)
            {
                if (!ThemeManager.TryGetThemeContext("#light", out IThemeContext? themeContext))
                    return;
                ThemeManager.CurrentTheme = themeContext;
            }
            else
            {
                if (!ThemeManager.TryGetThemeContext("#dark", out IThemeContext? themeContext))
                    return;
                ThemeManager.CurrentTheme = themeContext;
            }
        }

        private void LeftButton_Click(UIElement sender, in MouseInteractEventArgs args)
        {
            _progressBar!.Value -= 1.0f;
        }

        private void RightButton_Click(UIElement sender, in MouseInteractEventArgs args)
        {
            _progressBar!.Value += 1.0f;
        }

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            if (disposing)
            {
                foreach (UnwrappableList<UIElement> elementList in _elementLists)
                {
                    if (elementList is null)
                        continue;
                    foreach (UIElement element in elementList)
                    {
                        (element as IDisposable)?.Dispose();
                    }
                }
            }
            _ime?.Dispose();
        }
    }
}
