using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptExitStatement(
            JassMapScriptAdapterContext context,
            JassExitStatementSyntax exitStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedExitStatement)
        {
            if (TryAdaptExpression(context, exitStatement.Condition, out var adaptedCondition))
            {
                adaptedExitStatement = JassSyntaxFactory.ExitStatement(adaptedCondition);
                return true;
            }

            adaptedExitStatement = null;
            return false;
        }
    }
}