using System.Linq;

using War3App.MapAdapter.Diagnostics;

namespace War3App.MapAdapter.Extensions
{
    public static class AdaptResultExtensions
    {
        public static void Merge(this AdaptResult newAdaptResult, AdaptResult oldAdaptResult)
        {
            if (oldAdaptResult.AdaptedFileStream is not null)
            {
                if (newAdaptResult.AdaptedFileStream is null)
                {
                    newAdaptResult.AdaptedFileStream = oldAdaptResult.AdaptedFileStream;
                }
                else
                {
                    oldAdaptResult.Dispose();
                }
            }

            if (oldAdaptResult.NewFileName is not null)
            {
                if (newAdaptResult.NewFileName is null)
                {
                    newAdaptResult.NewFileName = oldAdaptResult.NewFileName;
                }
            }
        }

        public static DiagnosticSeverity GetDiagnosticSeverity(this AdaptResult? adaptResult)
        {
            return adaptResult?.Diagnostics is null || adaptResult.Diagnostics.Length == 0
                ? DiagnosticSeverity.Info
                : adaptResult.Diagnostics.Select(d => d.Descriptor.Severity).Max();
        }

        public static string? GetNewFileName(this AdaptResult? adaptResult)
        {
            return adaptResult is not null && adaptResult.Status != MapFileStatus.Removed
                ? adaptResult.NewFileName
                : null;
        }

        public static bool CanUndoChanges(this AdaptResult? adaptResult)
        {
            return adaptResult is not null
                && (adaptResult.AdaptedFileStream is not null || adaptResult.NewFileName is not null);
        }
    }
}