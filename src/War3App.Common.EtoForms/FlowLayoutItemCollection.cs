using System.Collections;
using System.Collections.ObjectModel;

using Eto.Drawing;
using Eto.Forms;

namespace War3App.Common.EtoForms
{
    partial class FlowLayoutPanel
    {
        private class FlowLayoutItemCollection : Collection<FlowLayoutItem>, IList
        {
            public FlowLayoutPanel Parent { get; }

            public FlowLayoutItemCollection(FlowLayoutPanel parent)
            {
                Parent = parent;
            }

            protected override void InsertItem(int index, FlowLayoutItem item)
            {
                base.InsertItem(index, item);
                if (item?.Control is not null)
                {
                    Parent.Add(item.Control, Point.Empty);
                }

                Parent.PerformLayout();
            }

            protected override void RemoveItem(int index)
            {
                var item = this[index];
                if (item?.Control is not null)
                {
                    Parent.Remove(item.Control);
                }

                base.RemoveItem(index);
                Parent.PerformLayout();
            }

            protected override void ClearItems()
            {
                foreach (var item in this)
                {
                    if (item?.Control is not null)
                    {
                        Parent.Remove(item.Control);
                    }
                }

                base.ClearItems();
                Parent.PerformLayout();
            }

            protected override void SetItem(int index, FlowLayoutItem item)
            {
                var last = this[index];
                if (last?.Control is not null)
                {
                    Parent.Remove(last.Control);
                }

                base.SetItem(index, item);
                if (item?.Control is not null)
                {
                    Parent.Add(item.Control, Point.Empty);
                }

                Parent.PerformLayout();
            }

            int IList.Add(object? value)
            {
                // allow adding a control directly from xaml
                if (value is Control control)
                {
                    Add(control);
                }
                else if (value is FlowLayoutItem item)
                {
                    Add(item);
                }
                else
                {
                    return -1;
                }

                return Count - 1;
            }
        }
    }
}