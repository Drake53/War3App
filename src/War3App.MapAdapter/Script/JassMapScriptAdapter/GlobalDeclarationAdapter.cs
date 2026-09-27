using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptIGlobalDeclaration(
            JassMapScriptAdapterContext context,
            JassGlobalDeclarationSyntax declaration,
            [NotNullWhen(true)] out JassGlobalDeclarationSyntax? adaptedGlobalDeclaration)
        {
            return declaration switch
            {
                JassGlobalVariableDeclarationSyntax globalDeclaration => TryAdaptGlobalDeclaration(context, globalDeclaration, out adaptedGlobalDeclaration),

                _ => TryAdaptDummy(context, declaration, out adaptedGlobalDeclaration),
            };
        }

        private bool TryAdaptGlobalDeclaration(
            JassMapScriptAdapterContext context,
            JassGlobalVariableDeclarationSyntax globalDeclaration,
            [NotNullWhen(true)] out JassGlobalDeclarationSyntax? adaptedGlobalDeclaration)
        {
            if (TryAdaptVariableDeclarator(context, true, globalDeclaration.Declarator, out var adaptedDeclarator))
            {
                adaptedGlobalDeclaration = JassSyntaxFactory.GlobalDeclaration(adaptedDeclarator);
                return true;
            }

            adaptedGlobalDeclaration = null;
            return false;
        }
    }
}