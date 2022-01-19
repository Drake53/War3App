using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptEqualsValueClause(JassMapScriptAdapterContext context, JassEqualsValueClauseSyntax? equalsValueClause, [NotNullWhen(true)] out JassEqualsValueClauseSyntax? adaptedEqualsValueClause)
        {
            if (equalsValueClause is not null &&
                TryAdaptExpression(context, equalsValueClause.Expression, out var adaptedExpression))
            {
                adaptedEqualsValueClause = new JassEqualsValueClauseSyntax(adaptedExpression);
                return true;
            }

            adaptedEqualsValueClause = null;
            return false;
        }
    }
}