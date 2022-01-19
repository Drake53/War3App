using System;
using System.Collections.Generic;

using War3Net.CodeAnalysis.Jass;

namespace War3App.MapAdapter.Script
{
    public class JassMapScriptAdapterContext
    {
        public JassMapScriptAdapterContext()
        {
            Diagnostics = new();
            KnownTypes = new(StringComparer.Ordinal);
            KnownFunctions = new(StringComparer.Ordinal);
            KnownGlobalVariables = new(StringComparer.Ordinal);
            KnownLocalVariables = new(StringComparer.Ordinal);
            DialogueTitles = new(StringComparer.Ordinal);
            DialogueTexts = new(StringComparer.Ordinal);

            KnownTypes.Add(JassKeyword.Integer, null);
            KnownTypes.Add(JassKeyword.Boolean, null);
            KnownTypes.Add(JassKeyword.Real, null);
            KnownTypes.Add(JassKeyword.String, null);
            KnownTypes.Add(JassKeyword.Handle, null);
            KnownTypes.Add(JassKeyword.Code, null);
        }

        public List<string> Diagnostics { get; set; }

        public Dictionary<string, string?> KnownTypes { get; set; }

        public Dictionary<string, string[]> KnownFunctions { get; set; }

        public Dictionary<string, string> KnownGlobalVariables { get; set; }

        public Dictionary<string, string> KnownLocalVariables { get; set; }

        public Dictionary<string, string> DialogueTitles { get; set; }

        public Dictionary<string, string> DialogueTexts { get; set; }
    }
}