using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptElseIfClause(JassMapScriptAdapterContext context, JassElseIfClauseSyntax elseIfClause, [NotNullWhen(true)] out JassElseIfClauseSyntax? adaptedElseIfClause)
        {
            if (TryAdaptExpression(context, elseIfClause.Condition, out var adaptedCondition) |
                TryAdaptStatementList(context, elseIfClause.Body, out var adaptedBody))
            {
                adaptedElseIfClause = new JassElseIfClauseSyntax(
                    adaptedCondition ?? elseIfClause.Condition,
                    adaptedBody ?? elseIfClause.Body);

                return true;
            }

            adaptedElseIfClause = null;
            return false;
        }
    }
}