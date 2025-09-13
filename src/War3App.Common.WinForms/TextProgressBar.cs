using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

// https://github.com/ukushu/TextProgressBar/blob/master/TextProgressBar.cs
namespace War3App.Common.WinForms
{
    [DesignerCategory("")]
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

        private SolidBrush _progressColourBrush = (SolidBrush)Brushes.LightGreen;
        private SolidBrush _textColourBrush = (SolidBrush)Brushes.Black;
        private Font _textFont = new Font(FontFamily.GenericSerif, 11, FontStyle.Bold /*| FontStyle.Italic*/);
        private string _text = string.Empty;
        private ProgressBarDisplayMode _visualMode = ProgressBarDisplayMode.CurrProgress;

        public TextProgressBar()
        {
            Value = Minimum;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Description("If it's empty, % will be shown"), Category("Additional Options"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public string CustomText
        {
            get => _text;
            set
            {
                _text = value;
                Invalidate(); // redraw component after change value from VS Properties section
            }
        }

        [Category("Additional Options"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public Color ProgressColor
        {
            get => _progressColourBrush.Color;
            set
            {
                _progressColourBrush.Dispose();
                _progressColourBrush = new SolidBrush(value);
            }
        }

        [Category("Additional Options")]
        public Color TextColor
        {
            get => _textColourBrush.Color;
            set
            {
                _textColourBrush.Dispose();
                _textColourBrush = new SolidBrush(value);
            }
        }

        [Description("Font of the text on ProgressBar"), Category("Additional Options")]
        public Font TextFont
        {
            get => _textFont;
            set => _textFont = value;
        }

        [Category("Additional Options"), Browsable(true)]
        public ProgressBarDisplayMode VisualMode
        {
            get => _visualMode;
            set
            {
                _visualMode = value;
                Invalidate(); // redraw component after change value from VS Properties section
            }
        }

        private string PercentageStr => $"{(int)((float)Value - Minimum) / ((float)Maximum - Minimum) * 100} %";

        private string CurrentProgressStr => $"{Value}/{Maximum}";

        public new void Dispose()
        {
            _textColourBrush.Dispose();
            _progressColourBrush.Dispose();
            base.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            DrawProgressBar(g);
            DrawStringIfNeeded(g);
        }

        private void DrawProgressBar(Graphics g)
        {
            var rect = ClientRectangle;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);

            rect.Inflate(-3, -3);

            if (Value > 0)
            {
                var clip = new Rectangle(
                    rect.X,
                    rect.Y,
                    (int)Math.Round(((float)Value / Maximum) * rect.Width),
                    rect.Height);

                g.FillRectangle(_progressColourBrush, clip);
            }
        }

        private void DrawStringIfNeeded(Graphics g)
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

            var len = g.MeasureString(text, TextFont);

            var location = new Point(
                (Width / 2) - (int)len.Width / 2,
                (Height / 2) - (int)len.Height / 2);

            g.DrawString(text, TextFont, _textColourBrush, location);
        }
    }
}