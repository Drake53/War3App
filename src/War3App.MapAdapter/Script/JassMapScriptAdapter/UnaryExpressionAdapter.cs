using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptUnaryExpression(
            JassMapScriptAdapterContext context,
            JassUnaryExpressionSyntax unaryExpression,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedUnaryExpression)
        {
            if (TryAdaptExpression(context, unaryExpression.Expression, out var adaptedExpression))
            {
                adaptedUnaryExpression = JassSyntaxFactory.UnaryNotExpression(
                    unaryExpression.OperatorToken,
                    adaptedExpression);

                return true;
            }

            adaptedUnaryExpression = null;
            return false;
        }
    }
}