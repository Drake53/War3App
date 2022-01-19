using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptLocalVariableDeclarationStatement(JassMapScriptAdapterContext context, JassLocalVariableDeclarationStatementSyntax localVariableDeclarationStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedLocalVariableDeclarationStatement)
        {
            if (TryAdaptVariableDeclarator(context, false, localVariableDeclarationStatement.Declarator, out var adaptedDeclarator))
            {
                adaptedLocalVariableDeclarationStatement = new JassLocalVariableDeclarationStatementSyntax(adaptedDeclarator);
                return true;
            }

            adaptedLocalVariableDeclarationStatement = null;
            return false;
        }
    }
}