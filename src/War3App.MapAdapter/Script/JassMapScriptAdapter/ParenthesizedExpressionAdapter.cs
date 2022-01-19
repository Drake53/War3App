using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptParenthesizedExpression(JassMapScriptAdapterContext context, JassParenthesizedExpressionSyntax parenthesizedExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedParenthesizedExpression)
        {
            if (TryAdaptExpression(context, parenthesizedExpression.Expression, out var adaptedExpression))
            {
                adaptedParenthesizedExpression = new JassParenthesizedExpressionSyntax(adaptedExpression);
                return true;
            }

            adaptedParenthesizedExpression = null;
            return false;
        }
    }
}