using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptElseClause(JassMapScriptAdapterContext context, JassElseClauseSyntax? elseClause, [NotNullWhen(true)] out JassElseClauseSyntax? adaptedElseClause)
        {
            if (elseClause is not null &&
                TryAdaptStatementList(context, elseClause.Body, out var adaptedBody))
            {
                adaptedElseClause = new JassElseClauseSyntax(adaptedBody);
                return true;
            }

            adaptedElseClause = null;
            return false;
        }
    }
}