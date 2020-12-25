using System.Windows.Forms;

namespace War3App.Common.WinForms.Extensions
{
    public static class ControlExtensions
    {
        public static void AddControls(this Control control, params Control[] controls)
        {
            control.Controls.AddRange(controls);
        }
    }
}