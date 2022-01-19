using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptExpression(JassMapScriptAdapterContext context, IExpressionSyntax? expression, [NotNullWhen(true)] out IExpressionSyntax? adaptedExpression)
        {
            return expression switch
            {
                JassFunctionReferenceExpressionSyntax functionReferenceExpression => TryAdaptFunctionReferenceExpression(context, functionReferenceExpression, out adaptedExpression),
                JassInvocationExpressionSyntax invocationExpression => TryAdaptInvocationExpression(context, invocationExpression, out adaptedExpression),
                JassArrayReferenceExpressionSyntax arrayReferenceExpression => TryAdaptArrayReferenceExpression(context, arrayReferenceExpression, out adaptedExpression),
                JassVariableReferenceExpressionSyntax variableReferenceExpression => TryAdaptVariableReferenceExpression(context, variableReferenceExpression, out adaptedExpression),
                JassParenthesizedExpressionSyntax parenthesizedExpression => TryAdaptParenthesizedExpression(context, parenthesizedExpression, out adaptedExpression),
                JassUnaryExpressionSyntax unaryExpression => TryAdaptUnaryExpression(context, unaryExpression, out adaptedExpression),
                JassBinaryExpressionSyntax binaryExpression => TryAdaptBinaryExpression(context, binaryExpression, out adaptedExpression),

                _ => TryAdaptDummy(context, expression, out adaptedExpression),
            };
        }
    }
}