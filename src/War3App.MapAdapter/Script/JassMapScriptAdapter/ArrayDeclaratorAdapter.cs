using System;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptArrayDeclarator(JassMapScriptAdapterContext context, bool isGlobalVariable, JassArrayDeclaratorSyntax arrayDeclarator, [NotNullWhen(true)] out IVariableDeclaratorSyntax? adaptedArrayDeclarator)
        {
            adaptedArrayDeclarator = null;
            return false;
        }
    }
}