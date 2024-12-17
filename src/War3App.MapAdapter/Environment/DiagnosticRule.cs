namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class MapCameras
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Cameras with local pitch/yaw/roll properties are not supported before {0}.",
            };

            public static readonly DiagnosticDescriptor RemovedCameraProperties = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "The local pitch/yaw/roll properties have been removed from camera {0}.",
            };

            public static readonly DiagnosticDescriptor DeletedCameraProperties = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "The local pitch/yaw/roll properties with non-default values {1}, {2}, {3} have been deleted from camera {0}.",
            };

            public static readonly DiagnosticDescriptor UnsupportedCameraProperties = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Camera '{0}' uses non-default values for unsupported properties local pitch/yaw/roll ({1}, {2}, {3}).",
            };
        }
    }
}