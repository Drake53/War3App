using Eto.Drawing;
using Eto.Forms;

namespace War3App.Common.EtoForms.Extensions
{
    public static class RichTextAreaExtensions
    {
        public static void Append(this RichTextArea richTextArea, string text, Color color)
        {
            var originalColor = richTextArea.SelectionForeground;

            richTextArea.Selection = new Range<int>(richTextArea.TextLength, richTextArea.TextLength);
            richTextArea.SelectionForeground = color;
            richTextArea.Append(text);
            richTextArea.SelectionForeground = originalColor;
        }

        public static void AppendLine(this RichTextArea richTextArea)
        {
            richTextArea.Append(System.Environment.NewLine);
        }

        public static void AppendLine(this RichTextArea richTextArea, string text, Color color)
        {
            richTextArea.Append(text, color);
            richTextArea.AppendLine();
        }
    }
}