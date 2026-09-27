using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptIfStatement(
            JassMapScriptAdapterContext context,
            JassIfStatementSyntax ifStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedIfStatement)
        {
            var isAdapted = false;

            var elseIfClausesBuilder = ImmutableArray.CreateBuilder<JassElseIfClauseSyntax>();
            foreach (var elseIfClause in ifStatement.ElseIfClauses)
            {
                if (TryAdaptElseIfClause(context, elseIfClause, out var adaptedElseIfClause))
                {
                    elseIfClausesBuilder.Add(adaptedElseIfClause);
                    isAdapted = true;
                }
                else
                {
                    elseIfClausesBuilder.Add(elseIfClause);
                }
            }

            if (TryAdaptExpression(context, ifStatement.IfClause.IfClauseDeclarator.Condition, out var adaptedCondition) |
                TryAdaptStatementList(context, ifStatement.IfClause.Statements, out var adaptedBody) |
                isAdapted |
                TryAdaptElseClause(context, ifStatement.ElseClause, out var adaptedElseClause))
            {
                adaptedIfStatement = JassSyntaxFactory.IfStatement(
                    adaptedCondition ?? ifStatement.IfClause.IfClauseDeclarator.Condition,
                    adaptedBody ?? ifStatement.IfClause.Statements,
                    isAdapted ? elseIfClausesBuilder.ToImmutable() : ifStatement.ElseIfClauses,
                    adaptedElseClause ?? ifStatement.ElseClause);

                return true;
            }

            adaptedIfStatement = null;
            return false;
        }
    }
}