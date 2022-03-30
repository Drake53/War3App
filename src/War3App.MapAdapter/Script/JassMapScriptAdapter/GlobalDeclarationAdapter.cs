using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptIGlobalDeclaration(JassMapScriptAdapterContext context, IGlobalDeclarationSyntax declaration, [NotNullWhen(true)] out IGlobalDeclarationSyntax? adaptedGlobalDeclaration)
        {
            return declaration switch
            {
                JassGlobalDeclarationSyntax globalDeclaration => TryAdaptGlobalDeclaration(context, globalDeclaration, out adaptedGlobalDeclaration),

                _ => TryAdaptDummy(context, declaration, out adaptedGlobalDeclaration),
            };
        }

        private bool TryAdaptGlobalDeclaration(JassMapScriptAdapterContext context, JassGlobalDeclarationSyntax globalDeclaration, [NotNullWhen(true)] out IGlobalDeclarationSyntax? adaptedGlobalDeclaration)
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