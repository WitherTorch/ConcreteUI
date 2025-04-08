using System.Collections.Generic;
using System.Drawing;

using ConcreteUI.Controls;
using ConcreteUI.Controls.Calculation;
using ConcreteUI.Input;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Collections;

using FormStartPosition = System.Windows.Forms.FormStartPosition;

namespace ConcreteUI.Test
{
    internal sealed class MainWindow : TabbedWindow
    {
        private readonly UnwrappableList<UIElement>[] _elementLists = new UnwrappableList<UIElement>[3];
        private InputMethod _ime;

        public MainWindow(CoreWindow parent) : base(parent, ["頁面A", "頁面B", "頁面C"])
        {
            InitializeBaseInformation();
        }

        private void InitializeBaseInformation()
        {
            ClientSize = new Size(950, 700);
            MinimumSize = new Size(640, 560);
            Text = nameof(MainWindow);
            StartPosition = FormStartPosition.CenterScreen;
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
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
            };
            button.WidthCalculation = new Button.AutoWidthCalculation(button);
            button.HeightCalculation = new Button.AutoHeightCalculation(button);
            button.Click += Button_Click;
            TextBox textBox = new TextBox(this, _ime)
            {
                LeftCalculation = new ElementDependedCalculation(button, LayoutProperty.Right, MarginType.Outside),
                TopCalculation = new PageDependedCalculation(LayoutProperty.Top),
                RightCalculation = new PageDependedCalculation(LayoutProperty.Right),
                HeightCalculation = new ElementDependedCalculation(button, LayoutProperty.Height, MarginType.None),
                Watermark = "這裡可以輸入文字喔!"
            };
            ListBox listBox = new ListBox(this)
            {
                Mode = ListBoxMode.Some,
                LeftCalculation = new PageDependedCalculation(LayoutProperty.Left),
                TopCalculation = new ElementDependedCalculation(button, LayoutProperty.Bottom, MarginType.Outside),
                WidthCalculation = new FixedCalculation(250),
                BottomCalculation = new PageDependedCalculation(LayoutProperty.Bottom),
            };
            for (int i = 1; i <= 200; i++)
                listBox.Items.Add("物件 " + i.ToString());
            elementList.Add(button);
            elementList.Add(textBox);
            elementList.Add(listBox);
            _elementLists[0] = elementList;
            _elementLists[1] = new();
            _elementLists[2] = new();
        }

        private void Button_Click(UIElement sender, in MouseInteractEventArgs args)
        {
            if (Theme.IsDarkTheme)
            {
                if (!ThemeManager.TryGetThemeContext("#light", out IThemeContext themeContext))
                    return;
                ThemeManager.CurrentTheme = themeContext;
            }
            else
            {
                if (!ThemeManager.TryGetThemeContext("#dark", out IThemeContext themeContext))
                    return;
                ThemeManager.CurrentTheme = themeContext;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _ime.Dispose();
        }
    }
}
