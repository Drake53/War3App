using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptElementAccessClause(
            JassMapScriptAdapterContext context,
            JassElementAccessClauseSyntax? elementAccessClause,
            [NotNullWhen(true)] out JassElementAccessClauseSyntax? adaptedElementAccessClause)
        {
            if (elementAccessClause is not null &&
                TryAdaptExpression(context, elementAccessClause.Expression, out var adaptedExpression))
            {
                adaptedElementAccessClause = JassSyntaxFactory.ElementAccessClause(adaptedExpression);
                return true;
            }

            adaptedElementAccessClause = null;
            return false;
        }
    }
}