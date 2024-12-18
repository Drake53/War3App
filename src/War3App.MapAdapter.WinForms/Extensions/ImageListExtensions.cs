using System.Drawing;
using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class ImageListExtensions
    {
        /// <returns>Index of the added image.</returns>
        public static int AddImage(this ImageList imageList, Image image)
        {
            var index = imageList.Images.Count;
            imageList.Images.Add(image);
            return index;
        }
    }
}