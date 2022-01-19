using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptUnaryExpression(JassMapScriptAdapterContext context, JassUnaryExpressionSyntax unaryExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedUnaryExpression)
        {
            if (TryAdaptExpression(context, unaryExpression.Expression, out var adaptedExpression))
            {
                adaptedUnaryExpression = new JassUnaryExpressionSyntax(
                    unaryExpression.Operator,
                    adaptedExpression);

                return true;
            }

            adaptedUnaryExpression = null;
            return false;
        }
    }
}