using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptStatementList(
            JassMapScriptAdapterContext context,
            ImmutableArray<JassStatementSyntax> statementList,
            [NotNullWhen(true)] out ImmutableArray<JassStatementSyntax>? adaptedStatementList)
        {
            var isAdapted = false;

            var statementsBuilder = ImmutableArray.CreateBuilder<JassStatementSyntax>();
            foreach (var statement in statementList)
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
                adaptedStatementList = statementsBuilder.ToImmutable();
                return true;
            }

            adaptedStatementList = null;
            return false;
        }
    }
}