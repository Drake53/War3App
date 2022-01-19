using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptArrayReferenceExpression(JassMapScriptAdapterContext context, JassArrayReferenceExpressionSyntax arrayReferenceExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedArrayReferenceExpression)
        {
            if (!context.KnownLocalVariables.ContainsKey(arrayReferenceExpression.IdentifierName.Name) &&
                !context.KnownGlobalVariables.ContainsKey(arrayReferenceExpression.IdentifierName.Name))
            {
                context.Diagnostics.Add($"Unknown array '{arrayReferenceExpression.IdentifierName}'.");
            }

            if (TryAdaptExpression(context, arrayReferenceExpression.Indexer, out var adaptedIndexer))
            {
                adaptedArrayReferenceExpression = new JassArrayReferenceExpressionSyntax(
                    arrayReferenceExpression.IdentifierName,
                    adaptedIndexer);

                return true;
            }

            adaptedArrayReferenceExpression = null;
            return false;
        }
    }
}