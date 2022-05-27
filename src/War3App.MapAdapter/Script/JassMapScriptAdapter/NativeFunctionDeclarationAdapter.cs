using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptNativeFunctionDeclaration(JassMapScriptAdapterContext context, JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration, [NotNullWhen(true)] out ITopLevelDeclarationSyntax? adaptedNativeFunctionDeclaration)
        {
            if (TryAdaptFunctionDeclarator(context, nativeFunctionDeclaration.FunctionDeclarator, out var adaptedFunctionDeclarator))
            {
                adaptedNativeFunctionDeclaration = new JassNativeFunctionDeclarationSyntax(adaptedFunctionDeclarator);
                return true;
            }

            adaptedNativeFunctionDeclaration = null;
            return false;
        }
    }
}