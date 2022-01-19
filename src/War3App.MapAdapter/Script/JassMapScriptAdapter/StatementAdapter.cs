using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptStatement(JassMapScriptAdapterContext context, IStatementSyntax statement, [NotNullWhen(true)] out IStatementSyntax? adaptedStatement)
        {
            return statement switch
            {
                JassLocalVariableDeclarationStatementSyntax localVariableDeclarationStatement => TryAdaptLocalVariableDeclarationStatement(context, localVariableDeclarationStatement, out adaptedStatement),
                JassSetStatementSyntax setStatement => TryAdaptSetStatement(context, setStatement, out adaptedStatement),
                JassCallStatementSyntax callStatement => TryAdaptCallStatement(context, callStatement, out adaptedStatement),
                JassIfStatementSyntax ifStatement => TryAdaptIfStatement(context, ifStatement, out adaptedStatement),
                JassLoopStatementSyntax loopStatement => TryAdaptLoopStatement(context, loopStatement, out adaptedStatement),
                JassExitStatementSyntax exitStatement => TryAdaptExitStatement(context, exitStatement, out adaptedStatement),
                JassReturnStatementSyntax returnStatement => TryAdaptReturnStatement(context, returnStatement, out adaptedStatement),
                JassDebugStatementSyntax debugStatement => TryAdaptDebugStatement(context, debugStatement, out adaptedStatement),

                _ => TryAdaptDummy(context, statement, out adaptedStatement),
            };
        }
    }
}