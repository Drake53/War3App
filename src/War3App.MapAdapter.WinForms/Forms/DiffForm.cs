using System.ComponentModel;
using System.Windows.Forms;

using DiffPlex.WindowsForms.Controls;

namespace War3App.MapAdapter.WinForms.Forms
{
    [DesignerCategory("")]
    public class DiffForm : Form
    {
        public DiffForm(string oldText, string newText)
        {
            var diffViewer = new DiffViewer
            {
                Dock = DockStyle.Fill,
                OldText = oldText,
                NewText = newText,
            };

            WindowState = FormWindowState.Maximized;

            Controls.Add(diffViewer);
        }
    }
}