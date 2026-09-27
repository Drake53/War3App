using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptVariableReferenceExpression(
            JassMapScriptAdapterContext context,
            JassIdentifierNameSyntax identifierName,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedIdentifierName)
        {
            if (!context.KnownLocalVariables.ContainsKey(identifierName.Token.Text) &&
                !context.KnownGlobalVariables.ContainsKey(identifierName.Token.Text))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableReferenceUnknownIdentifier, identifierName.Token.Text);
            }

            adaptedIdentifierName = null;
            return false;
        }
    }
}