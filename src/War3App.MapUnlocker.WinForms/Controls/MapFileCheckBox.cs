using System.ComponentModel;
using System.Windows.Forms;

using War3Net.Build;

namespace War3App.MapUnlocker.WinForms.Controls
{
    [DesignerCategory("")]
    public class MapFileCheckBox : CheckBox
    {
        public MapFileCheckBox(MapFiles mapFile, string fileName)
        {
            MapFile = mapFile;
            Text = $"{mapFile} ({fileName})";
        }

        public MapFiles MapFile { get; }
    }
}