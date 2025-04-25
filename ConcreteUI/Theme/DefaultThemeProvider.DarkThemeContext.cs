using System;
using System.Collections.Generic;
using System.Drawing;

using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Theme
{
    partial class DefaultThemeProvider
    {
        private sealed class DarkThemeContext : ThemeContextBase
        {
            public override bool IsDarkTheme => true;

            public DarkThemeContext() { }

            private DarkThemeContext(DarkThemeContext original) : base(original) { }

            public override IThemeContext Clone() => new DarkThemeContext(this);

            protected override IEnumerable<KeyValuePair<string, IThemedColorFactory>> CreateColorFactories(Func<string, IThemedColorFactory> queryFunction)
            {
                yield return new KeyValuePair<string, IThemedColorFactory>(
                    key: ThemeConstants.WindowBaseColorNode,
                    value: ThemedColorFactory.FromColor(new D2D1ColorF(40, 40, 40)));
                yield return new KeyValuePair<string, IThemedColorFactory>(
                    key: ThemeConstants.ClearDCColorNode,
                    value: ThemedColorFactory.CreateBuilder(new D2D1ColorF(64, 64, 64))
                        .WithVariant(WindowMaterial.MicaAlt, Color.Transparent)
                        .WithVariant(WindowMaterial.Mica, Color.Transparent)
                        .WithVariant(WindowMaterial.Acrylic, new D2D1ColorF(64, 64, 64, 72))
                        .WithVariant(WindowMaterial.Gaussian, new D2D1ColorF(64, 64, 64, 145))
                        .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(0, 0, 0, 0))
                        .Build());
                yield return new KeyValuePair<string, IThemedColorFactory>(
                    key: ThemeConstants.WizardWindowBaseColor,
                    value: ThemedColorFactory.CreateBuilder(new D2D1ColorF(64, 64, 64))
                        .WithVariant(WindowMaterial.MicaAlt, Color.Transparent)
                        .WithVariant(WindowMaterial.Mica, Color.Transparent)
                        .WithVariant(WindowMaterial.Acrylic, new D2D1ColorF(64, 64, 64, 72))
                        .WithVariant(WindowMaterial.Gaussian, new D2D1ColorF(64, 64, 64, 145))
                        .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(40, 40, 40, 96))
                        .Build());
            }

            protected override IEnumerable<KeyValuePair<string, IThemedBrushFactory>> CreateBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunction, Func<string, IThemedBrushFactory> queryBrushFunction)
            {
                // 視窗基礎筆刷
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.title.back",
                    value: ThemedBrushFactory.CreateBuilder(Color.Black)
                        .WithVariant(WindowMaterial.MicaAlt, Color.Transparent)
                        .WithVariant(WindowMaterial.Mica, Color.Transparent)
                        .WithVariant(WindowMaterial.Acrylic, new D2D1ColorF(20, 20, 20, 32))
                        .WithVariant(WindowMaterial.Gaussian, new D2D1ColorF(20, 20, 20, 64))
                        .Build());
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.menu.back",
                    value: ThemedBrushFactory.CreateBuilder(queryBrushFunction("app.title.back"))
                        .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(20, 20, 20, 100))
                        .Build());
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.title.fore.active",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(245, 245, 245)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.title.fore.deactive",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(175, 175, 175)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.title.closeButton.active",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(241, 59, 75)));

                // 通用元件
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.back",
                    value: ThemedBrushFactory.FromColorFactory(queryColorFunction(ThemeConstants.WindowBaseColorNode)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.back.disabled",
                    value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunction("app.control.back"), 0.7f));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.back.hovered",
                    value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunction("app.control.back"), 1.1f));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.border",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(165, 165, 165)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.border.active",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(96, 162, 252)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.fore",
                    value: ThemedBrushFactory.FromColor(Color.White));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.fore.inactive",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(203, 203, 203)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.control.fore.description",
                    value: ThemedBrushFactory.FromColor(Color.LightGray));

                // 目錄
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.menu.fore",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(210, 210, 210)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.menu.fore.active",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(230, 230, 230)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.menu.itemSelected.back",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(71, 119, 155, 235)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.menu.itemHovered.back",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(71, 119, 155, 135)));

                // 右鍵選單
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.back",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.back.hovered",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(200, 200, 200)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.back.pressed",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.fore",
                    value: queryBrushFunction("app.control.fore"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.fore.inactive",
                    value: queryBrushFunction("app.control.fore.inactive"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.contextMenu.fore.hovered",
                    value: ThemedBrushFactory.FromColor(Color.White));

                // 標籤
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.label.fore",
                    value: queryBrushFunction("app.control.fore"));

                // 按鈕
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.border.hovered",
                    value: queryBrushFunction("app.control.border.active"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.face",
                    value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunction("app.control.back"), 1.5f));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.face.hovered",
                    value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunction("app.control.back"), 1.2f));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.face.pressed",
                    value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunction("app.control.back"), 0.7f));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.fore",
                    value: queryBrushFunction("app.control.fore"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.button.fore.inactive",
                    value: queryBrushFunction("app.control.fore.inactive"));

                // 字型圖示按鈕
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.fontIconButton.face",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.fontIconButton.face.hovered",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(96, 162, 252)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.fontIconButton.face.pressed",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(127, 180, 252)));

                // 核取方塊
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.border.hovered",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(190, 190, 190)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.border.pressed",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(175, 175, 175)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.border.checked",
                    value: queryBrushFunction("app.checkBox.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.border.hovered.checked",
                    value: queryBrushFunction("app.checkBox.border.hovered"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.border.pressed.checked",
                    value: queryBrushFunction("app.checkBox.border.pressed"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.mark",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.checkBox.fore",
                    value: queryBrushFunction("app.control.fore"));

                // 卷軸
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.scrollBar.back",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(50, 50, 50)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.scrollBar.fore",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(149, 149, 149)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.scrollBar.fore.hovered",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(164, 164, 164)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.scrollBar.fore.pressed",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(200, 200, 200)));

                // 文字方塊
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.back",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.back.disabled",
                    value: queryBrushFunction("app.control.back.disabled"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.border.focused",
                    value: queryBrushFunction("app.control.border.active"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.fore",
                    value: queryBrushFunction("app.control.fore"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.fore.inactive",
                    value: queryBrushFunction("app.control.fore.inactive"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.selection.back",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(0, 120, 215)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.textbox.selection.fore",
                    value: ThemedBrushFactory.FromColor(Color.White));

                // 下拉式方塊
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.back",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.back.disabled",
                    value: queryBrushFunction("app.control.back.disabled"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.back.hovered",
                    value: queryBrushFunction("app.control.back.hovered"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.fore",
                    value: queryBrushFunction("app.control.fore"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.list.back.hovered",
                    value: queryBrushFunction("app.contextMenu.back.hovered"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.list.back.pressed",
                    value: queryBrushFunction("app.contextMenu.back.pressed"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.list.fore.hovered",
                    value: queryBrushFunction("app.contextMenu.fore.hovered"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.button",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(215, 215, 215)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.button.hovered",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(190, 190, 190)));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.comboBox.button.pressed",
                    value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));

                // 進度條
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.progressBar.back",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.progressBar.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.progressBar.fore",
                    value: queryBrushFunction("app.control.fore.inactive"));

                // 群組方塊
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.groupBox.back",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.groupBox.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.groupBox.fore",
                    value: queryBrushFunction("app.control.fore"));

                // 列表方塊
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.listbox.back",
                    value: queryBrushFunction("app.control.back"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.listbox.back.disabled",
                    value: queryBrushFunction("app.control.back.disabled"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.listbox.border",
                    value: queryBrushFunction("app.control.border"));
                yield return new KeyValuePair<string, IThemedBrushFactory>(
                    key: "app.listbox.fore",
                    value: queryBrushFunction("app.control.fore"));
            }
        }
    }
}
