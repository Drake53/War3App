using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptBinaryExpression(
            JassMapScriptAdapterContext context,
            JassBinaryExpressionSyntax binaryExpression,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedBinaryExpression)
        {
            if (TryAdaptExpression(context, binaryExpression.Left, out var adaptedLeftExpression) |
                TryAdaptExpression(context, binaryExpression.Right, out var adaptedRightExpression))
            {
                adaptedBinaryExpression = JassSyntaxFactory.BinaryExpression(
                    adaptedLeftExpression ?? binaryExpression.Left,
                    binaryExpression.OperatorToken,
                    adaptedRightExpression ?? binaryExpression.Right);

                return true;
            }

            adaptedBinaryExpression = null;
            return false;
        }
    }
}