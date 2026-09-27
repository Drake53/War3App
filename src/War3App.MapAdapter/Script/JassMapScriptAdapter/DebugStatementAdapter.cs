using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptDebugStatement(
            JassMapScriptAdapterContext context,
            JassDebugStatementSyntax debugStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedDebugStatement)
        {
            if (TryAdaptStatement(context, debugStatement.Statement, out var adaptedStatement))
            {
                adaptedDebugStatement = JassSyntaxFactory.DebugStatement(adaptedStatement);
                return true;
            }

            adaptedDebugStatement = null;
            return false;
        }
    }
}