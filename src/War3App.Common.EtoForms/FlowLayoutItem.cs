using System;

using Eto.Forms;

namespace War3App.Common.EtoForms
{
    /// <summary>
    /// Item for a single control in a <see cref="FlowLayoutPanel"/>.
    /// </summary>
    public class FlowLayoutItem
    {
        /// <summary>
        /// Gets or sets the control for this item.
        /// </summary>
        /// <value>The item's control.</value>
        public Control? Control { get; set; }

        /// <summary>
        /// Gets or sets the minimum size for the control in the direction of the layout.
        /// </summary>
        /// <value>The minimum size in pixels. A value >= 0 causes the control to expand to fill available space with at least this minimum size. Use -1 for no expansion.</value>
        public int MinimumSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowLayoutItem"/> class.
        /// </summary>
        /// <param name="control">Control for the item.</param>
        /// <param name="minimumSize">The minimum size for the control in the direction of the layout. Use -1 for no expansion.</param>
        public FlowLayoutItem(Control? control, int minimumSize = -1)
        {
            Control = control;
            MinimumSize = minimumSize;
        }

        /// <summary>
        /// Converts a <paramref name="control"/> to a <see cref="FlowLayoutItem"/> implicitly.
        /// </summary>
        /// <param name="control">The control to convert.</param>
        public static implicit operator FlowLayoutItem(Control control)
        {
            return new FlowLayoutItem(control);
        }
    }
}