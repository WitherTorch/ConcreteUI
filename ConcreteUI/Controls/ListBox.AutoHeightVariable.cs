using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class ListBox
    {
        private sealed class AutoHeightVariable : UIElementDependedVariable<ListBox>
        {
            public AutoHeightVariable(ListBox element) : base(element) { }

            protected override int Compute(ListBox element, in LayoutVariableManager manager) => element.GetPredictedHeight();
        }
    }
}
