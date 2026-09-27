using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptParenthesizedExpression(
            JassMapScriptAdapterContext context,
            JassParenthesizedExpressionSyntax parenthesizedExpression,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedParenthesizedExpression)
        {
            if (TryAdaptExpression(context, parenthesizedExpression.Expression, out var adaptedExpression))
            {
                adaptedParenthesizedExpression = JassSyntaxFactory.ParenthesizedExpression(adaptedExpression);
                return true;
            }

            adaptedParenthesizedExpression = null;
            return false;
        }
    }
}