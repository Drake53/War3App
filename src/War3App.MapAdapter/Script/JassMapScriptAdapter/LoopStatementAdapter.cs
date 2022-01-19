using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptLoopStatement(JassMapScriptAdapterContext context, JassLoopStatementSyntax loopStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedLoopStatement)
        {
            if (TryAdaptStatementList(context, loopStatement.Body, out var adaptedBody))
            {
                adaptedLoopStatement = new JassLoopStatementSyntax(adaptedBody);
                return true;
            }

            adaptedLoopStatement = null;
            return false;
        }
    }
}