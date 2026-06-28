using System.Collections.Generic;

using ShioUI.Controls;
using ShioUI.Controls.Extensions;
using ShioUI.Layout;
using ShioUI.Input;

namespace ShioUI.Test;

partial class MainWindow
{
    private readonly List<UIElement>[] _elementLists = new List<UIElement>[3];
    private InputMethod? _ime;
    private ProgressBar? _progressBar;

    protected override IEnumerable<UIElement?> GetActiveElements(uint pageIndex)
        => _elementLists[pageIndex];

    protected override void InitializeElements()
    {
        _ime = new InputMethod(this);

        InitializeFirstPageElements();
        InitializeSecondPageElements();
        InitializeThirdPageElements();
    }

    private void InitializeFirstPageElements()
    {
        List<UIElement> elementList = new List<UIElement>();

        Button button = new Button(this)
        {
            Text = "請點我!",
            LeftExpression = UIConstants.ElementMargin,
            TopExpression = UIConstants.ElementMargin,
        }.WithAutoWidth().WithAutoHeight();
        button.Click += Button_Click;
        elementList.Add(button);

        TextBox textBox = new TextBox(this, _ime)
        {
            LeftExpression = button.RightDefinition + UIConstants.ElementMargin,
            TopExpression = UIConstants.ElementMargin,
            RightExpression = PageWidthDefinition - UIConstants.ElementMargin,
            HeightExpression = button.HeightDefinition,
            Watermark = "這裡可以輸入文字喔!"
        };
        elementList.Add(textBox);

        ListBox listBox = new ListBox(this)
        {
            Mode = ListBoxMode.Some,
            LeftExpression = UIConstants.ElementMargin,
            TopExpression = textBox.BottomDefinition + UIConstants.ElementMargin,
            WidthExpression = 250,
            BottomExpression = PageHeightDefinition - UIConstants.ElementMargin,
        };
        IList<string> items = listBox.Items;
        for (int i = 1; i <= 200; i++)
            items.Add("物件 " + i.ToString());
        elementList.Add(listBox);

        GroupBox groupBox = new GroupBox(this)
        {
            LeftExpression = listBox.RightDefinition + UIConstants.ElementMargin,
            TopExpression = textBox.BottomDefinition + UIConstants.ElementMargin,
            RightExpression = PageWidthDefinition - UIConstants.ElementMargin,
            BottomExpression = PageHeightDefinition - UIConstants.ElementMargin,
            Title = "群組容器",
        };
        InitializeGroupBox(groupBox);
        elementList.Add(groupBox);
        _elementLists[0] = elementList;

        void InitializeGroupBox(GroupBox groupBox)
        {
            groupBox.AddChild(new CheckBox(this)
            {
                LeftExpression = groupBox.ContentLeftDefinition,
                TopExpression = groupBox.ContentTopDefinition,
                Text = "可以勾選的方塊"
            }.WithAutoWidth().WithAutoHeight());

            ComboBox comboBox = new ComboBox(this)
            {
                LeftExpression = groupBox.ContentLeftDefinition,
                TopExpression = groupBox.FirstChild!.BottomDefinition + UIConstants.ElementMargin,
                WidthExpression = 200
            }.WithAutoHeight();
            Label label = new Label(this)
            {
                LeftExpression = groupBox.ContentLeftDefinition,
                TopExpression = comboBox.BottomDefinition + UIConstants.ElementMargin,
                RightExpression = groupBox.ContentRightDefinition,
                Text = "底下是進度條測試",
                Alignment = TextAlignment.MiddleCenter
            }.WithAutoHeight();
            Button leftButton = new Button(this)
            {
                LeftExpression = label.LeftDefinition,
                TopExpression = label.BottomDefinition + UIConstants.ElementMargin,
                Text = "-"
            }.WithAutoWidth().WithAutoHeight();
            Button rightButton = new Button(this)
            {
                TopExpression = label.BottomDefinition + UIConstants.ElementMargin,
                RightExpression = label.RightDefinition,
                Text = "+"
            }.WithAutoWidth().WithAutoHeight();
            ProgressBar progressBar = new ProgressBar(this)
            {
                LeftExpression = leftButton.RightDefinition + UIConstants.ElementMargin,
                TopExpression = label.BottomDefinition + UIConstants.ElementMargin,
                RightExpression = rightButton.LeftDefinition - UIConstants.ElementMargin,
                HeightExpression = leftButton.HeightDefinition,
                Maximium = 100.0f,
                Value = 50.0f
            };
            TextBox textBox2 = new TextBox(this, _ime)
            {
                LeftExpression = groupBox.ContentLeftDefinition,
                TopExpression = progressBar.BottomDefinition + UIConstants.ElementMargin,
                RightExpression = groupBox.ContentRightDefinition,
                Watermark = "這裡也可以輸入文字喔!"
            }.WithAutoHeight();
            groupBox.AddChildren(comboBox, label, leftButton, rightButton, progressBar, textBox2);

            _progressBar = progressBar;
            comboBox.RequestDropdownListOpening += ComboBox_RequestDropdownListOpening;
            items = comboBox.Items;
            for (int i = 1; i <= 200; i++)
                items.Add("選項 " + i.ToString());
            leftButton.Click += LeftButton_Click;
            rightButton.Click += RightButton_Click;
        }
    }

    private void InitializeSecondPageElements()
    {
        List<UIElement> elementList = new List<UIElement>();
        TextBox textbox = new TextBox(this, _ime)
        {
            LeftExpression = UIConstants.ElementMarginDefinition,
            TopExpression = UIConstants.ElementMarginDefinition,
            RightExpression = PageWidthDefinition - UIConstants.ElementMarginDefinition,
            Watermark = "盡情輸入文字吧!",
            MultiLine = true
        };
        textbox.HeightExpression = LayoutNode.Min(textbox.AutoHeightDefinition, PageHeightDefinition / 2 - textbox.TopDefinition);
        textbox.TextChanging += (object sender, ref TextChangingEventArgs _) => (sender as UIElement)?.ResetLayoutTimestamp(); // 強制讓元件下次渲染時重算布局

        Label label = new Label(this)
        {
            Text = "不過這條標籤將永遠在文字框的底下，而且文字框最大只能到頁面的一半高度",
            Alignment = TextAlignment.MiddleCenter,
            LeftExpression = textbox.LeftDefinition,
            TopExpression = textbox.BottomDefinition + UIConstants.ElementMarginDefinition,
            RightExpression = textbox.RightDefinition,
        }.WithAutoHeight();

        elementList.Add(textbox);
        elementList.Add(label);
        _elementLists[1] = elementList;
    }

    private void InitializeThirdPageElements()
    {
        List<UIElement> elementList = new List<UIElement>();
        Button rollingButton = new Button(this)
        {
            Text = "按鈕!"
        }.WithAutoWidth().WithAutoHeight();
        rollingButton.LeftExpression = (PageWidthDefinition - rollingButton.WidthDefinition) / 2;
        rollingButton.TopExpression = (PageHeightDefinition - rollingButton.HeightDefinition) / 2;
        rollingButton.Click += RollingButton_Click;
        elementList.Add(rollingButton);
        _elementLists[2] = elementList;
    }
}
