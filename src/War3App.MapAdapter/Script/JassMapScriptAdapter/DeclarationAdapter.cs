﻿using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptDeclaration(JassMapScriptAdapterContext context, ITopLevelDeclarationSyntax declaration, [NotNullWhen(true)] out ITopLevelDeclarationSyntax? adaptedDeclaration)
        {
            return declaration switch
            {
                JassGlobalDeclarationListSyntax globalDeclarationList => TryAdaptGlobalDeclarationList(context, globalDeclarationList, out adaptedDeclaration),
                JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration => TryAdaptNativeFunctionDeclaration(context, nativeFunctionDeclaration, out adaptedDeclaration),
                JassFunctionDeclarationSyntax functionDeclaration => TryAdaptFunctionDeclaration(context, functionDeclaration, out adaptedDeclaration),
                JassTypeDeclarationSyntax typeDeclaration => TryAdaptTypeDeclaration(context, typeDeclaration, out adaptedDeclaration),

                _ => TryAdaptDummy(context, declaration, out adaptedDeclaration),
            };
        }
    }
}