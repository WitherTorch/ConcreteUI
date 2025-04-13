using System;

namespace ConcreteUI.Controls.Calculation
{
    public enum OffsetType
    {
        /// <summary>
        /// Result = input
        /// </summary>
        None,
        /// <summary>
        /// Result = input + margin
        /// </summary>
        Add,
        /// <summary>
        /// Result = input - margin
        /// </summary>
        Subtract,
        /// <summary>
        /// Inside the element depending
        /// </summary>
        Inside,
        /// <summary>
        /// Outside the element depending
        /// </summary>
        Outside
    }

    [Flags]
    public enum CalculationDependFlags
    {
        None = 0x0,
        PageRect = 0x1,
        Elements = 0x2,
    }
}
