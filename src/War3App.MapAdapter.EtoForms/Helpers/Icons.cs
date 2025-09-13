using System;

using Eto.Drawing;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;

namespace War3App.MapAdapter.EtoForms.Helpers
{
    internal static class Icons
    {
        private static readonly Lazy<Bitmap> _bubble = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Bubble.png");
        private static readonly Lazy<Bitmap> _copy = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Copy.png");
        private static readonly Lazy<Bitmap> _delete = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Delete.png");
        private static readonly Lazy<Bitmap> _download = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Download.png");
        private static readonly Lazy<Bitmap> _error = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Error.png");
        private static readonly Lazy<Bitmap> _lightning = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Lightning.png");
        private static readonly Lazy<Bitmap> _lock = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Lock.png");
        private static readonly Lazy<Bitmap> _modify = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Modify.png");
        private static readonly Lazy<Bitmap> _ok = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.OK.png");
        private static readonly Lazy<Bitmap> _question = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Question.png");
        private static readonly Lazy<Bitmap> _trash = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Trash.png");
        private static readonly Lazy<Bitmap> _undo = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Undo.png");
        private static readonly Lazy<Bitmap> _warning = LoadEmbeddedImage("War3App.MapAdapter.EtoForms.Resources.Icons.Warning.png");

        internal static Bitmap Bubble => _bubble.Value;

        internal static Bitmap Copy => _copy.Value;

        internal static Bitmap Delete => _delete.Value;

        internal static Bitmap Download => _download.Value;

        internal static Bitmap Error => _error.Value;

        internal static Bitmap Lightning => _lightning.Value;

        internal static Bitmap Lock => _lock.Value;

        internal static Bitmap Modify => _modify.Value;

        internal static Bitmap OK => _ok.Value;

        internal static Bitmap Question => _question.Value;

        internal static Bitmap Trash => _trash.Value;

        internal static Bitmap Undo => _undo.Value;

        internal static Bitmap Warning => _warning.Value;

        internal static Bitmap ForMapFile(MapFile mapFile)
        {
            return ForStatus(mapFile.Status, mapFile.AdaptResult.GetDiagnosticSeverity());
        }

        internal static Bitmap ForStatus(MapFileStatus status, DiagnosticSeverity severity = DiagnosticSeverity.Info)
        {
            return severity switch
            {
                DiagnosticSeverity.Error => Error,
                DiagnosticSeverity.Warning => Warning,
                _ => status switch
                {
                    MapFileStatus.Removed => Trash,
                    MapFileStatus.Unknown => Question,
                    MapFileStatus.Compatible => OK,
                    MapFileStatus.Inconclusive => Warning,
                    MapFileStatus.Pending => Bubble,
                    MapFileStatus.Incompatible => Warning,
                    MapFileStatus.Locked => Lock,
                    MapFileStatus.Error => Error,
                },
            };
        }

        private static Lazy<Bitmap> LoadEmbeddedImage(string resourceName)
        {
            return new Lazy<Bitmap>(() => Bitmap.FromResource(resourceName, typeof(Icons)));
        }
    }
}