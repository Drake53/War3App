using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptFunctionReferenceExpression(JassMapScriptAdapterContext context, JassFunctionReferenceExpressionSyntax functionReferenceExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedFunctionReferenceExpression)
        {
            if (context.KnownFunctions.TryGetValue(functionReferenceExpression.IdentifierName.Name, out var knownFunctionParameters))
            {
                if (knownFunctionParameters.Length != 0)
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionReferenceHasParameters, functionReferenceExpression.IdentifierName, knownFunctionParameters.Length);
                }
            }
            else
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionReferenceUnknownIdentifier, functionReferenceExpression.IdentifierName);
            }

            adaptedFunctionReferenceExpression = null;
            return false;
        }
    }
}