using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    internal static class FormExtensions
    {
        public static void AddControls(this Form form, params Control[] controls)
        {
            form.Controls.AddRange(controls);
        }
    }
}