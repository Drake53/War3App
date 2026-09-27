using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptElseIfClause(
            JassMapScriptAdapterContext context,
            JassElseIfClauseSyntax elseIfClause,
            [NotNullWhen(true)] out JassElseIfClauseSyntax? adaptedElseIfClause)
        {
            if (TryAdaptExpression(context, elseIfClause.ElseIfClauseDeclarator.Condition, out var adaptedCondition) |
                TryAdaptStatementList(context, elseIfClause.Statements, out var adaptedBody))
            {
                adaptedElseIfClause = JassSyntaxFactory.ElseIfClause(
                    adaptedCondition ?? elseIfClause.ElseIfClauseDeclarator.Condition,
                    adaptedBody ?? elseIfClause.Statements);

                return true;
            }

            adaptedElseIfClause = null;
            return false;
        }
    }
}