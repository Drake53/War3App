using System;

using Eto.Drawing;

using Eto.Forms;

namespace War3App.Common.EtoForms
{
    public class ProgressBar : Drawable
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private Color _backgroundColor = Colors.Silver;
        private Color _borderColor = Colors.Gray;
        private Color _progressColor = Colors.LightGreen;

        public ProgressBar()
        {
            Paint += OnPaint;
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                if (_value < _minimum)
                {
                    _value = _minimum;
                }

                Invalidate();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                if (_value > _maximum)
                {
                    _value = _maximum;
                }

                Invalidate();
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Clamp(value, _minimum, _maximum);
                Invalidate();
            }
        }

        public new Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public Color BorderColor
        {
            get => _borderColor;
            set => _borderColor = value;
        }

        public Color ProgressColor
        {
            get => _progressColor;
            set => _progressColor = value;
        }

        protected virtual void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new RectangleF(0, 0, Size.Width, Size.Height);

            DrawProgressBar(g, rect);
        }

        protected void DrawProgressBar(Graphics g, RectangleF rect)
        {
            g.FillRectangle(BackgroundColor, rect);
            g.DrawRectangle(BorderColor, rect);

            if (Value > Minimum)
            {
                var clip = new RectangleF(
                    rect.X + 1.5f,
                    rect.Y + 1.5f,
                    (float)(Value - Minimum) / (Maximum - Minimum) * (rect.Width - 1.5f),
                    rect.Height - 1.5f);

                g.FillRectangle(ProgressColor, clip);
            }
        }
    }
}