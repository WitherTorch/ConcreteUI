using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

using ConcreteUI.Controls;
using ConcreteUI.Input;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Collections;

namespace ConcreteUI.Test;

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
        if (!Screen.TryGetBoundsCenteredScreen(handle, out Rectangle bounds))
            return;
        RawBounds = bounds;
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

    protected override IEnumerable<UIElement> GetActiveElements(uint pageIndex)
        => _elementLists[pageIndex];

    protected override void InitializeElements()
    {
        _ime = new InputMethod(this);
        UnwrappableList<UIElement> elementList = new UnwrappableList<UIElement>();

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
        elementList.Add(groupBox);

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
        comboBox.RequestDropdownListOpening += ComboBox_RequestDropdownListOpening;
        items = comboBox.Items;
        for (int i = 1; i <= 200; i++)
            items.Add("選項 " + i.ToString());
        groupBox.AddChild(comboBox);
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

    private void Button_Click(UIElement sender, in MouseEventArgs args)
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

    private void LeftButton_Click(UIElement sender, in MouseEventArgs args)
    {
        _progressBar!.Value -= 1.0f;
    }

    private void RightButton_Click(UIElement sender, in MouseEventArgs args)
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
