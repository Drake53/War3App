using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using War3App.Common.EtoForms.Extensions;

namespace War3App.Common.EtoForms
{
    /// <summary>
    /// Alternative to <see cref="StackLayout"/> with support for wrapping content.
    /// </summary>
    public partial class FlowLayoutPanel : PixelLayout
    {
        private readonly FlowLayoutItemCollection _items;
        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        private bool _wrapContents = true;
        private Padding _padding;
        private Size _spacing;
        private bool _spacersHaveEqualPriority;
        private int _suspended;
        private bool _isCreated = false;

        public FlowLayoutPanel()
        {
            _items = new(this);
        }

        public Collection<FlowLayoutItem> Items => _items;

        /// <summary>
        /// Gets the controls for the layout.
        /// </summary>
        public override IEnumerable<Control> Controls => Items.Select(item => item.Control).WhereNotNull();

        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                if (_flowDirection != value)
                {
                    _flowDirection = value;
                    InvalidateLayout();
                }
            }
        }

        public bool WrapContents
        {
            get => _wrapContents;
            set
            {
                if (_wrapContents != value)
                {
                    _wrapContents = value;
                    InvalidateLayout();
                }
            }
        }

        public Padding Padding
        {
            get => _padding;
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    InvalidateLayout();
                }
            }
        }

        public Size Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    InvalidateLayout();
                }
            }
        }

        public bool SpacersHaveEqualPriority
        {
            get => _spacersHaveEqualPriority;
            set
            {
                if (_spacersHaveEqualPriority != value)
                {
                    _spacersHaveEqualPriority = value;
                    InvalidateLayout();
                }
            }
        }

        public void Add(Control control)
        {
            if (control is not null)
            {
                _items.Add(control);
                Add(control, Point.Empty);
                PerformLayout();
            }
        }

        public override void Remove(Control control)
        {
            if (control is not null && _items.Remove(control))
            {
                base.Remove(control);
                PerformLayout();
            }
        }

        protected override void OnPreLoad(EventArgs e)
        {
            PerformInitialLayout();
            base.OnPreLoad(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            PerformInitialLayout();
            base.OnLoad(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            PerformLayout();
        }

        public override void SuspendLayout()
        {
            base.SuspendLayout();
            _suspended++;
        }

        public override void ResumeLayout()
        {
            if (_suspended == 0)
            {
                throw new InvalidOperationException("Must balance ResumeLayout with SuspendLayout calls");
            }

            _suspended--;
            base.ResumeLayout();
            PerformLayout();
        }

        private void InvalidateLayout()
        {
            if (!_isCreated)
            {
                return;
            }

            PerformLayout();
        }

        private void PerformInitialLayout()
        {
            if (_isCreated)
            {
                return;
            }

            PerformLayout();
        }

        private void PerformLayout()
        {
            if (_suspended > 0)
            {
                return;
            }

            if (_items.Count == 0)
            {
                return;
            }

            var availableSize = Size;
            if (availableSize.Width <= 0 || availableSize.Height <= 0)
            {
                return;
            }

            var isHorizontal = FlowDirection is FlowDirection.LeftToRight or FlowDirection.RightToLeft;
            var isReversed = FlowDirection is FlowDirection.RightToLeft or FlowDirection.BottomUp;
            var limit = isHorizontal
                ? availableSize.Width - _padding.Horizontal
                : availableSize.Height - _padding.Vertical;

            if (isHorizontal)
            {
                LayoutHorizontal(isReversed, limit);
            }
            else
            {
                LayoutVertical(isReversed, limit);
            }

            _isCreated = true;
        }

        private void LayoutHorizontal(bool rightToLeft, int availableWidth)
        {
            var rows = new List<FlowLayoutRow>();
            var sizes = new List<Size>();
            FlowLayoutRow? currentRow = null;

            // Group items into rows
            foreach (var item in _items)
            {
                if (item.Control is null && item.MinimumSize < 0)
                {
                    currentRow ??= new();
                    currentRow.IncludeItem(item, 0);
                    rows.Add(currentRow);
                    currentRow = null;

                    sizes.Add(Size.Empty);
                    continue;
                }

                var preferredSize = item.Control is not null
                    ? GetControlSize(item.Control)
                    : Size.Empty;

                var size = item.MinimumSize >= 0
                    ? new Size(item.MinimumSize, preferredSize.Height)
                    : preferredSize;

                sizes.Add(size);

                if (currentRow is null)
                {
                    currentRow = new();
                    currentRow.IncludeItem(item, size.Width);
                }
                else
                {
                    var requiredWidth = size.Width;
                    if (item.Control is not null)
                    {
                        requiredWidth += _spacing.Width;
                    }

                    if (!_wrapContents || currentRow.ReservedSize + requiredWidth <= availableWidth)
                    {
                        currentRow.IncludeItem(item, requiredWidth);
                    }
                    else
                    {
                        rows.Add(currentRow);
                        currentRow = new();
                        currentRow.IncludeItem(item, size.Width);
                    }
                }
            }

            if (currentRow is not null)
            {
                rows.Add(currentRow);
            }

            var i = 0;
            var y = _padding.Top;

            foreach (var row in rows)
            {
                var remainingSpace = availableWidth - row.ReservedSize;
                var expandSizePerControl = 0;
                var expandSizePerSpacer = 0;

                if (remainingSpace > 0 && (row.ExpandableControlCount > 0 || row.SpacerCount > 0))
                {
                    if (_spacersHaveEqualPriority)
                    {
                        expandSizePerControl = remainingSpace / (row.ExpandableControlCount + row.SpacerCount);
                        expandSizePerSpacer = expandSizePerControl;
                    }
                    else if (row.ExpandableControlCount > 0)
                    {
                        expandSizePerControl = remainingSpace / row.ExpandableControlCount;
                    }
                    else
                    {
                        expandSizePerSpacer = remainingSpace / row.SpacerCount;
                    }
                }

                var x = rightToLeft ? -_padding.Left : _padding.Left;
                var rowHeight = 0;

                for (var col = 0; col < row.TotalCount; col++)
                {
                    var item = _items[i];
                    var size = sizes[i];

                    i++;

                    if (item.Control is null)
                    {
                        if (item.MinimumSize >= 0)
                        {
                            x += item.MinimumSize;
                            x += expandSizePerSpacer;
                        }

                        continue;
                    }

                    var finalSize = item.MinimumSize >= 0
                        ? new Size(item.MinimumSize + expandSizePerControl, size.Height)
                        : size;

                    if (rightToLeft)
                    {
                        Move(item.Control, new Point(availableWidth - finalSize.Width - x, y));
                        item.Control.Size = finalSize;
                    }
                    else
                    {
                        Move(item.Control, new Point(x, y));
                        item.Control.Size = finalSize;
                    }

                    x += finalSize.Width;
                    x += _spacing.Width;

                    rowHeight = Math.Max(rowHeight, finalSize.Height);
                }

                y += rowHeight + _spacing.Height;
            }
        }

        private void LayoutVertical(bool bottomUp, int availableHeight)
        {
            var columns = new List<FlowLayoutRow>();
            var sizes = new List<Size>();
            FlowLayoutRow? currentColumn = null;

            // Group items into columns
            foreach (var item in _items)
            {
                if (item.Control is null && item.MinimumSize < 0)
                {
                    currentColumn ??= new();
                    currentColumn.IncludeItem(item, 0);
                    columns.Add(currentColumn);
                    currentColumn = null;

                    sizes.Add(Size.Empty);
                    continue;
                }

                var preferredSize = item.Control is not null
                    ? GetControlSize(item.Control)
                    : Size.Empty;

                var size = item.MinimumSize >= 0
                    ? new Size(preferredSize.Width, item.MinimumSize)
                    : preferredSize;

                sizes.Add(size);

                if (currentColumn is null)
                {
                    currentColumn = new();
                    currentColumn.IncludeItem(item, size.Height);
                }
                else
                {
                    var requiredHeight = size.Height;
                    if (item.Control is not null)
                    {
                        requiredHeight += _spacing.Height;
                    }

                    if (!_wrapContents || currentColumn.ReservedSize + requiredHeight <= availableHeight)
                    {
                        currentColumn.IncludeItem(item, requiredHeight);
                    }
                    else
                    {
                        columns.Add(currentColumn);
                        currentColumn = new();
                        currentColumn.IncludeItem(item, size.Height);
                    }
                }
            }

            if (currentColumn is not null)
            {
                columns.Add(currentColumn);
            }

            var i = 0;
            var x = _padding.Left;

            foreach (var column in columns)
            {
                var remainingSpace = availableHeight - column.ReservedSize;
                var expandSizePerControl = 0;
                var expandSizePerSpacer = 0;

                if (remainingSpace > 0 && (column.ExpandableControlCount > 0 || column.SpacerCount > 0))
                {
                    if (_spacersHaveEqualPriority)
                    {
                        expandSizePerControl = remainingSpace / (column.ExpandableControlCount + column.SpacerCount);
                        expandSizePerSpacer = expandSizePerControl;
                    }
                    else if (column.ExpandableControlCount > 0)
                    {
                        expandSizePerControl = remainingSpace / column.ExpandableControlCount;
                    }
                    else
                    {
                        expandSizePerSpacer = remainingSpace / column.SpacerCount;
                    }
                }

                var y = bottomUp ? -_padding.Top : _padding.Top;
                var columnWidth = 0;

                for (var row = 0; row < column.TotalCount; row++)
                {
                    var item = _items[i];
                    var size = sizes[i];

                    i++;

                    if (item.Control is null)
                    {
                        if (item.MinimumSize >= 0)
                        {
                            y += item.MinimumSize;
                            y += expandSizePerSpacer;
                        }

                        continue;
                    }

                    var finalSize = item.MinimumSize >= 0
                        ? new Size(size.Width, item.MinimumSize + expandSizePerControl)
                        : size;

                    if (bottomUp)
                    {
                        Move(item.Control, new Point(x, availableHeight - finalSize.Height - y));
                        item.Control.Size = finalSize;
                    }
                    else
                    {
                        Move(item.Control, new Point(x, y));
                        item.Control.Size = finalSize;
                    }

                    y += finalSize.Height;
                    y += _spacing.Height;

                    columnWidth = Math.Max(columnWidth, finalSize.Width);
                }

                x += columnWidth + _spacing.Width;
            }
        }

        private static Size GetControlSize(Control control)
        {
            var preferredSize = control.GetPreferredSize(SizeF.Empty);
            return new Size((int)preferredSize.Width, (int)preferredSize.Height);
        }

        private class FlowLayoutRow
        {
            public int ReservedSize { get; set; }

            public int ExpandableControlCount { get; set; }

            public int SpacerCount { get; set; }

            public int TotalCount { get; set; }

            public void IncludeItem(FlowLayoutItem item, int size)
            {
                if (item.MinimumSize >= 0)
                {
                    if (item.Control is not null)
                    {
                        ExpandableControlCount++;
                    }
                    else
                    {
                        SpacerCount++;
                    }
                }

                TotalCount++;
                ReservedSize += size;
            }
        }
    }
}