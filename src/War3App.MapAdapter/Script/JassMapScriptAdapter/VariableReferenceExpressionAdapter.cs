using System.Diagnostics.CodeAnalysis;

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
                context.Diagnostics.Add($"Unknown variable '{variableReferenceExpression.IdentifierName}'.");
            }

            adaptedVariableReferenceExpression = null;
            return false;
        }
    }
}