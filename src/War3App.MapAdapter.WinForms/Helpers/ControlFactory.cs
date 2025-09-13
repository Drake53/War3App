using System.Drawing;
using System.Windows.Forms;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;

namespace War3App.MapAdapter.WinForms.Helpers
{
    public static class ControlFactory
    {
        public static Button Button(string text, Image? image = null, bool enabled = false)
        {
            var button = new Button
            {
                Enabled = enabled,
                Image = image,
                Text = text,
            };

            button.Size = button.PreferredSize;
            return button;
        }

        public static ComboBox DropDownList(int width)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false,
                Width = width,
            };
        }

        public static FlowLayoutPanel HorizontalLayoutPanel(params Control[] controls)
        {
            var flowLayoutPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            flowLayoutPanel.AddControls(controls);
            return flowLayoutPanel;
        }

        public static Label Label(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.BottomRight,
            };
        }

        public static RichTextBox RichTextBox()
        {
            return new RichTextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
            };
        }

        public static TextBox TextBox(string placeholderText)
        {
            return new TextBox
            {
                PlaceholderText = placeholderText,
            };
        }

        public static TextProgressBar TextProgressBar()
        {
            return new TextProgressBar
            {
                Dock = DockStyle.Bottom,
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                VisualMode = Common.WinForms.TextProgressBar.ProgressBarDisplayMode.CustomText,
            };
        }

        public static FlowLayoutPanel VerticalLayoutPanel(int width, params Control[] controls)
        {
            var flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                Width = width,
            };

            flowLayoutPanel.AddControls(controls);
            return flowLayoutPanel;
        }
    }
}