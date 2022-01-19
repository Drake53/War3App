using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptDebugStatement(JassMapScriptAdapterContext context, JassDebugStatementSyntax debugStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedDebugStatement)
        {
            if (TryAdaptStatement(context, debugStatement.Statement, out var adaptedStatement))
            {
                adaptedDebugStatement = new JassDebugStatementSyntax(adaptedStatement);
                return true;
            }

            adaptedDebugStatement = null;
            return false;
        }
    }
}