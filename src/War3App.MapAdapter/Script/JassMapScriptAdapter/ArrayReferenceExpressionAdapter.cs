using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptElementAccessExpression(
            JassMapScriptAdapterContext context,
            JassElementAccessExpressionSyntax arrayReferenceExpression,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedArrayReferenceExpression)
        {
            if (!context.KnownLocalVariables.ContainsKey(arrayReferenceExpression.IdentifierName.Token.Text) &&
                !context.KnownGlobalVariables.ContainsKey(arrayReferenceExpression.IdentifierName.Token.Text))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.ArrayReferenceUnknownIdentifier, arrayReferenceExpression.IdentifierName);
            }

            if (TryAdaptExpression(context, arrayReferenceExpression.ElementAccessClause.Expression, out var adaptedIndexer))
            {
                adaptedArrayReferenceExpression = JassSyntaxFactory.ElementAccessExpression(
                    arrayReferenceExpression.IdentifierName,
                    adaptedIndexer);

                return true;
            }

            adaptedArrayReferenceExpression = null;
            return false;
        }
    }
}