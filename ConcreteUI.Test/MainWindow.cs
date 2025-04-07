using System.Collections.Generic;
using System.Drawing;

using ConcreteUI.Controls;
using ConcreteUI.Controls.Calculation;
using ConcreteUI.Input;
using ConcreteUI.Theme;
using ConcreteUI.Window;

using WitherTorch.Common.Collections;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

using FormStartPosition = System.Windows.Forms.FormStartPosition;

namespace ConcreteUI.Test
{
    internal sealed class MainWindow : TabbedWindow
    {
        private readonly UnwrappableList<UIElement>[] _elementLists = new UnwrappableList<UIElement>[3];
        private InputMethod _ime;

        public MainWindow(CoreWindow parent) : base(parent, ["頁面A"])
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
                TopCalculation = new PageDependedCalculation(LayoutProperty.Top),
                HeightCalculation = new FixedCalculation(26)
            };
            TextBox textBox = new TextBox(this, _ime)
            {
                LeftCalculation = new ElementDependedCalculation(button, LayoutProperty.Right, MarginType.Outside),
                TopCalculation = new PageDependedCalculation(LayoutProperty.Top),
                RightCalculation = new PageDependedCalculation(LayoutProperty.Right),
                HeightCalculation = new FixedCalculation(26),
                FontSize = 14,
            };
            button.WidthCalculation = new Button.AutoWidthCalculation(button);
            elementList.Add(button);
            elementList.Add(textBox);
            _elementLists[0] = elementList;
            _elementLists[1] = new();
            _elementLists[2] = new();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _ime.Dispose();
        }
    }
}
