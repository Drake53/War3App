using System.Drawing;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class BitmapExtensions
    {
        public static void SetSolidColor(this Bitmap bitmap, Color color)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, color);
                }
            }
        }

        public static Bitmap WithSolidColor(this Bitmap bitmap, Color color)
        {
            bitmap.SetSolidColor(color);
            return bitmap;
        }
    }
}