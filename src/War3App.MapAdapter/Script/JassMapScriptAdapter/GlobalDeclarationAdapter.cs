using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptGlobalDeclaration(JassMapScriptAdapterContext context, JassGlobalDeclarationSyntax globalDeclaration, [NotNullWhen(true)] out IDeclarationSyntax? adaptedGlobalDeclaration)
        {
            if (TryAdaptVariableDeclarator(context, true, globalDeclaration.Declarator, out var adaptedDeclarator))
            {
                adaptedGlobalDeclaration = new JassGlobalDeclarationSyntax(adaptedDeclarator);
                return true;
            }

            adaptedGlobalDeclaration = null;
            return false;
        }
    }
}