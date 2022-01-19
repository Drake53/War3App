using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptStatementList(JassMapScriptAdapterContext context, JassStatementListSyntax statementList, [NotNullWhen(true)] out JassStatementListSyntax? adaptedStatementList)
        {
            var isAdapted = false;

            var statementsBuilder = ImmutableArray.CreateBuilder<IStatementSyntax>();
            foreach (var statement in statementList.Statements)
            {
                if (TryAdaptStatement(context, statement, out var adaptedStatement))
                {
                    statementsBuilder.Add(adaptedStatement);
                    isAdapted = true;
                }
                else
                {
                    statementsBuilder.Add(statement);
                }
            }

            if (isAdapted)
            {
                adaptedStatementList = new JassStatementListSyntax(statementsBuilder.ToImmutable());
                return true;
            }

            adaptedStatementList = null;
            return false;
        }
    }
}