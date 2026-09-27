using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptLoopStatement(
            JassMapScriptAdapterContext context,
            JassLoopStatementSyntax loopStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedLoopStatement)
        {
            if (TryAdaptStatementList(context, loopStatement.Statements, out var adaptedBody))
            {
                adaptedLoopStatement = JassSyntaxFactory.LoopStatement(adaptedBody);
                return true;
            }

            adaptedLoopStatement = null;
            return false;
        }
    }
}