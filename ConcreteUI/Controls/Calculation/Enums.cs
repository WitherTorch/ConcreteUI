using System;

namespace ConcreteUI.Controls.Calculation
{
    public enum MarginType
    {
        /// <summary>
        /// Outside the element depending
        /// </summary>
        Outside = -2,
        /// <summary>
        /// Inside the element depending
        /// </summary>
        Inside = -1,
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
        Subtract
    }

    [Flags]
    public enum CalculationDependFlags
    {
        None = 0x0,
        PageRect = 0x1,
        Elements = 0x2,
    }
}
