using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptExpression(
            JassMapScriptAdapterContext context,
            JassExpressionSyntax? expression,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedExpression)
        {
            return expression switch
            {
                JassFunctionReferenceExpressionSyntax functionReferenceExpression => TryAdaptFunctionReferenceExpression(context, functionReferenceExpression, out adaptedExpression),
                JassInvocationExpressionSyntax invocationExpression => TryAdaptInvocationExpression(context, invocationExpression, out adaptedExpression),
                JassElementAccessExpressionSyntax elementAccessExpression => TryAdaptElementAccessExpression(context, elementAccessExpression, out adaptedExpression),
                JassIdentifierNameSyntax identifierName => TryAdaptVariableReferenceExpression(context, identifierName, out adaptedExpression),
                JassParenthesizedExpressionSyntax parenthesizedExpression => TryAdaptParenthesizedExpression(context, parenthesizedExpression, out adaptedExpression),
                JassUnaryExpressionSyntax unaryExpression => TryAdaptUnaryExpression(context, unaryExpression, out adaptedExpression),
                JassBinaryExpressionSyntax binaryExpression => TryAdaptBinaryExpression(context, binaryExpression, out adaptedExpression),

                _ => TryAdaptDummy(context, expression, out adaptedExpression),
            };
        }
    }
}