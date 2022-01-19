using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptExitStatement(JassMapScriptAdapterContext context, JassExitStatementSyntax exitStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedExitStatement)
        {
            if (TryAdaptExpression(context, exitStatement.Condition, out var adaptedCondition))
            {
                adaptedExitStatement = new JassExitStatementSyntax(adaptedCondition);
                return true;
            }

            adaptedExitStatement = null;
            return false;
        }
    }
}