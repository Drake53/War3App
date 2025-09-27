using Eto.Drawing;
using Eto.Forms;
using War3App.Common.EtoForms;
using War3App.MapAdapter.EtoForms.Controls;
using War3App.MapAdapter.EtoForms.Forms;

namespace War3App.MapAdapter.EtoForms.Helpers
{
    public static class ControlFactory
    {
        public static Button Button(string text, int width = -1, Image? image = null, bool enabled = false)
        {
            return new Button
            {
                Enabled = enabled,
                Image = image,
                Text = text,
                Size = new Size(width, -1),
            };
        }

        public static DiagnosticsDisplay DiagnosticsDisplay()
        {
            return new DiagnosticsDisplay();
        }

        public static ComboBox DropDownList(int width)
        {
            return new ComboBox
            {
                ReadOnly = true,
                Enabled = false,
                Size = new Size(width, -1),
            };
        }

        public static FileTreeView FileTreeView(MainForm mainForm)
        {
            return new FileTreeView(mainForm);
        }

        public static FlowLayoutPanel HorizontalFlowLayout(params FlowLayoutItem[] items)
        {
            var stackLayout = new FlowLayoutPanel
            {
                Padding = new Padding(15),
                Spacing = new Size(5, 5),
            };

            foreach (var item in items)
            {
                stackLayout.Items.Add(item);
            }

            return stackLayout;
        }

        public static Label Label(string text)
        {
            return new Label
            {
                Text = text,
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
        }

        public static Splitter Splitter(int position, int panel1MinimumSize)
        {
            return new Splitter
            {
                Orientation = Orientation.Horizontal,
                FixedPanel = SplitterFixedPanel.Panel1,
                Position = position,
                Panel1MinimumSize = panel1MinimumSize,
            };
        }

        public static TextBox TextBox(string placeholderText, int width = -1)
        {
            return new TextBox
            {
                PlaceholderText = placeholderText,
                Size = new Size(width, -1),
            };
        }

        public static TextProgressBar TextProgressBar()
        {
            return new TextProgressBar
            {
                Visible = false,
                VisualMode = Common.EtoForms.TextProgressBar.ProgressBarDisplayMode.CustomText,
            };
        }
    }
}