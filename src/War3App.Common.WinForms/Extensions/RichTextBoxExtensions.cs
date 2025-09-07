using System.Drawing;
using System.Windows.Forms;

namespace War3App.Common.WinForms.Extensions
{
    public static class RichTextBoxExtensions
    {
        public static void Write(this RichTextBox richTextBox, string text, Color color)
        {
            var originalColor = richTextBox.SelectionColor;

            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = originalColor;
        }

        public static void WriteLine(this RichTextBox richTextBox)
        {
            richTextBox.AppendText(System.Environment.NewLine);
        }

        public static void WriteLine(this RichTextBox richTextBox, string text, Color color)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.AppendText(System.Environment.NewLine);
        }
    }
}