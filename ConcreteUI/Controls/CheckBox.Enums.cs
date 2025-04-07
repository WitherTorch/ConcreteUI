namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private enum CheckBoxDrawType : long
        {
            NoRedraw,
            RedrawCheckBox,
            RedrawAllContent
        }

        private enum Brush
        {
            BorderBrush,
            BorderHoveredBrush,
            BorderPressedBrush,
            BorderCheckedBrush,
            BorderHoveredCheckedBrush,
            BorderPressedCheckedBrush,
            MarkBrush,
            TextBrush,
            _Last
        }
    }
}
