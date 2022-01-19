using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptReturnStatement(JassMapScriptAdapterContext context, JassReturnStatementSyntax returnStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedReturnStatement)
        {
            if (TryAdaptExpression(context, returnStatement.Value, out var adaptedValue))
            {
                adaptedReturnStatement = new JassReturnStatementSyntax(adaptedValue);
                return true;
            }

            adaptedReturnStatement = null;
            return false;
        }
    }
}