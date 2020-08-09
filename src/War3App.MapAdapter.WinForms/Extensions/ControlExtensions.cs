using System.Windows.Forms;

namespace War3App.MapAdapter.WinForms.Extensions
{
    internal static class ControlExtensions
    {
        public static void AddControls(this Control control, params Control[] controls)
        {
            control.Controls.AddRange(controls);
        }
    }
}