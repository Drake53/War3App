using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptVariableReferenceExpression(JassMapScriptAdapterContext context, JassVariableReferenceExpressionSyntax variableReferenceExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedVariableReferenceExpression)
        {
            if (!context.KnownLocalVariables.ContainsKey(variableReferenceExpression.IdentifierName.Name) &&
                !context.KnownGlobalVariables.ContainsKey(variableReferenceExpression.IdentifierName.Name))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableReferenceUnknownIdentifier, variableReferenceExpression.IdentifierName);
            }

            adaptedVariableReferenceExpression = null;
            return false;
        }
    }
}