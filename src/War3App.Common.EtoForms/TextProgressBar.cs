using Eto.Drawing;
using Eto.Forms;

namespace War3App.Common.EtoForms
{
    public class TextProgressBar : ProgressBar
    {
        public enum ProgressBarDisplayMode
        {
            NoText,
            Percentage,
            CurrProgress,
            CustomText,
            TextAndPercentage,
            TextAndCurrProgress,
        }

        private Color _textColor = Colors.Black;
        private Font _textFont = SystemFonts.Default();
        private string _text = string.Empty;
        private ProgressBarDisplayMode _visualMode = ProgressBarDisplayMode.CurrProgress;

        public TextProgressBar()
        {
            Value = Minimum;
        }

        public string CustomText
        {
            get => _text;
            set
            {
                _text = value;
                Invalidate();
            }
        }

        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        public Font TextFont
        {
            get => _textFont;
            set => _textFont = value;
        }

        public ProgressBarDisplayMode VisualMode
        {
            get => _visualMode;
            set
            {
                _visualMode = value;
                Invalidate();
            }
        }

        private string PercentageStr => $"{(int)((float)Value - Minimum) / ((float)Maximum - Minimum) * 100} %";

        private string CurrentProgressStr => $"{Value}/{Maximum}";

        protected override void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new RectangleF(0, 0, Size.Width, Size.Height);

            DrawProgressBar(g, rect);
            DrawStringIfNeeded(g, rect);
        }

        private void DrawStringIfNeeded(Graphics g, RectangleF rect)
        {
            if (VisualMode == ProgressBarDisplayMode.NoText)
            {
                return;
            }

            var text = VisualMode switch
            {
                ProgressBarDisplayMode.Percentage => PercentageStr,
                ProgressBarDisplayMode.CurrProgress => CurrentProgressStr,
                ProgressBarDisplayMode.TextAndPercentage => $"{CustomText}: {PercentageStr}",
                ProgressBarDisplayMode.TextAndCurrProgress => $"{CustomText}: {CurrentProgressStr}",
                _ => CustomText,
            };

            var len = g.MeasureString(TextFont, text);

            var location = new PointF(
                (rect.Width - len.Width) / 2,
                (rect.Height - len.Height) / 2);

            g.DrawText(TextFont, TextColor, location, text);
        }
    }
}