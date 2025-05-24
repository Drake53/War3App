using War3Net.Build.Common;

namespace War3App.MapAdapter
{
    public class TargetPatch
    {
        public GamePatch Patch { get; set; }

        public string GameDataPath { get; set; }

        public ContainerType GameDataContainerType { get; set; }
    }
}