using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public class AdaptFileContext
    {
        private readonly List<Diagnostic> _diagnostics = new();

        public string? FileName { get; set; }

        public MpqArchive Archive { get; set; }

        public TargetPatch TargetPatch { get; set; }

        public GamePatch OriginPatch { get; set; }

        public void ReportDiagnostic(DiagnosticDescriptor diagnostic, params object?[] arguments)
        {
            _diagnostics.Add(new Diagnostic
            {
                Descriptor = diagnostic,
                FileName = FileName,
                Message = string.Format(diagnostic.Description, arguments),
            });
        }

        public void ReportRegexDiagnostic(DiagnosticDescriptor diagnostic, Regex regex, params object?[] arguments)
        {
            _diagnostics.Add(new Diagnostic
            {
                Descriptor = diagnostic,
                Regex = regex,
                FileName = FileName,
                Message = string.Format(diagnostic.Description, arguments),
            });
        }

        public MapFileStatus ReportParseError(Exception e)
        {
            ReportDiagnostic(DiagnosticRule.General.ParseError, e.GetTypeAndMessage());

            return MapFileStatus.ParseError;
        }

        public MapFileStatus ReportAdapterError(Exception e)
        {
            ReportDiagnostic(DiagnosticRule.General.AdapterError, e.GetTypeAndMessage());

            return MapFileStatus.AdapterError;
        }

        public MapFileStatus ReportSerializeError(Exception e)
        {
            ReportDiagnostic(DiagnosticRule.General.SerializerError, e.GetTypeAndMessage());

            return MapFileStatus.SerializeError;
        }

        public Diagnostic[] GetDiagnostics()
        {
            return _diagnostics.ToArray();
        }
    }
}