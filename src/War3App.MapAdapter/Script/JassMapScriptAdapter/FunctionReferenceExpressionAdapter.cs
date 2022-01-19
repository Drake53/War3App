using System.Diagnostics.CodeAnalysis;

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
                    context.Diagnostics.Add($"Invalid function reference: '{functionReferenceExpression.IdentifierName}' should not have any parameters.");
                }
            }
            else
            {
                context.Diagnostics.Add($"Unknown function '{functionReferenceExpression.IdentifierName}'.");
            }

            adaptedFunctionReferenceExpression = null;
            return false;
        }
    }
}