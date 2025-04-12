using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

using ConcreteUI.Controls;
using ConcreteUI.Controls.Calculation;
using ConcreteUI.Input;
using ConcreteUI.Theme;
using ConcreteUI.Window;

using WitherTorch.Common.Collections;

using FormStartPosition = System.Windows.Forms.FormStartPosition;

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

        private void InitializeBaseInformation()
        {
            ClientSize = new Size(950, 700);
            MinimumSize = new Size(640, 560);
            Text = nameof(MainWindow);
            StartPosition = FormStartPosition.CenterScreen;
            Stream? stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("ConcreteUI.Test.app-icon.ico");
            if (stream is not null)
            {
                Icon = new Icon(stream);
                stream.Dispose();
            }
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
                LeftCalculation = new PageDependedCalculation(LayoutProperty.Left),
                TopCalculation = new PageDependedCalculation(LayoutProperty.Top)
            }.WithAutoWidthCalculation().WithAutoHeightCalculation();
            button.Click += Button_Click;
            elementList.Add(button);

            TextBox textBox = new TextBox(this, _ime)
            {
                LeftCalculation = new ElementDependedCalculation(button, LayoutProperty.Right, MarginType.Outside),
                TopCalculation = new PageDependedCalculation(LayoutProperty.Top),
                RightCalculation = new PageDependedCalculation(LayoutProperty.Right),
                HeightCalculation = new ElementDependedCalculation(button, LayoutProperty.Height, MarginType.None),
                Watermark = "這裡可以輸入文字喔!"
            };
            elementList.Add(textBox);

            ListBox listBox = new ListBox(this)
            {
                Mode = ListBoxMode.Some,
                LeftCalculation = new PageDependedCalculation(LayoutProperty.Left),
                TopCalculation = new ElementDependedCalculation(button, LayoutProperty.Bottom, MarginType.Outside),
                WidthCalculation = new FixedCalculation(250),
                BottomCalculation = new PageDependedCalculation(LayoutProperty.Bottom),
            };
            IList<string> items = listBox.Items;
            for (int i = 1; i <= 200; i++)
                items.Add("物件 " + i.ToString());
            elementList.Add(listBox);

            GroupBox groupBox = new GroupBox(this)
            {
                LeftCalculation = new ElementDependedCalculation(listBox, LayoutProperty.Right, MarginType.Outside),
                TopCalculation = new ElementDependedCalculation(textBox, LayoutProperty.Bottom, MarginType.Outside),
                RightCalculation = new PageDependedCalculation(LayoutProperty.Right),
                BottomCalculation = new PageDependedCalculation(LayoutProperty.Bottom),
                Title = "群組容器",
            };
            elementList.Add(groupBox);

            groupBox.AddChild(new CheckBox(this)
            {
                LeftCalculation = new GroupBox.ContentXCalculation(groupBox),
                TopCalculation = new GroupBox.ContentYCalculation(groupBox),
                Text = "可以勾選的方塊"
            }.WithAutoWidthCalculation().WithAutoHeightCalculation());

            ComboBox comboBox = new ComboBox(this)
            {
                LeftCalculation = new GroupBox.ContentXCalculation(groupBox),
                TopCalculation = new ElementDependedCalculation(groupBox.FirstChild!, LayoutProperty.Bottom, MarginType.Outside),
                WidthCalculation = new FixedCalculation(200)
            }.WithAutoHeightCalculation();
            comboBox.RequestDropdownListOpening += ComboBox_RequestDropdownListOpening;
            items = comboBox.Items;
            for (int i = 1; i <= 200; i++)
                items.Add("選項 " + i.ToString());
            groupBox.AddChild(comboBox);
            Label label = new Label(this)
            {
                LeftCalculation = new GroupBox.ContentXCalculation(groupBox),
                TopCalculation = new ElementDependedCalculation(comboBox, LayoutProperty.Bottom, MarginType.Outside),
                RightCalculation = new ElementDependedCalculation(groupBox, LayoutProperty.Right),
                Text = "底下是進度條測試",
                Alignment = TextAlignment.MiddleCenter
            }.WithAutoHeightCalculation();
            Button leftButton = new Button(this)
            {
                LeftCalculation = new ElementDependedCalculation(label, LayoutProperty.Left, MarginType.None),
                TopCalculation = new ElementDependedCalculation(label, LayoutProperty.Bottom, MarginType.Outside),
                Text = "-"
            }.WithAutoWidthCalculation().WithAutoHeightCalculation();
            Button rightButton = new Button(this)
            {
                TopCalculation = new ElementDependedCalculation(label, LayoutProperty.Bottom, MarginType.Outside),
                RightCalculation = new ElementDependedCalculation(label, LayoutProperty.Right, MarginType.None),
                Text = "+"
            }.WithAutoWidthCalculation().WithAutoHeightCalculation();
            ProgressBar progressBar = new ProgressBar(this)
            {
                LeftCalculation = new ElementDependedCalculation(leftButton, LayoutProperty.Right, MarginType.Outside),
                TopCalculation = new ElementDependedCalculation(label, LayoutProperty.Bottom, MarginType.Outside),
                RightCalculation = new ElementDependedCalculation(rightButton, LayoutProperty.Left, MarginType.Outside),
                HeightCalculation = new ElementDependedCalculation(leftButton, LayoutProperty.Height, MarginType.None),
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
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
