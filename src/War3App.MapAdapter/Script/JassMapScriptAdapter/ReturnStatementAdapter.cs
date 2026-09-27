using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptReturnStatement(
            JassMapScriptAdapterContext context,
            JassReturnStatementSyntax returnStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedReturnStatement)
        {
            if (TryAdaptExpression(context, returnStatement.Value, out var adaptedValue))
            {
                adaptedReturnStatement = JassSyntaxFactory.ReturnStatement(adaptedValue);
                return true;
            }

            adaptedReturnStatement = null;
            return false;
        }
    }
}