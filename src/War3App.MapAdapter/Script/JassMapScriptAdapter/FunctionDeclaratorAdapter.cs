﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptFunctionDeclarator(JassMapScriptAdapterContext context, JassFunctionDeclaratorSyntax functionDeclarator, [NotNullWhen(true)] out JassFunctionDeclaratorSyntax? adaptedFunctionDeclarator)
        {
            if (!context.KnownFunctions.TryAdd(
                functionDeclarator.IdentifierName.Name,
                functionDeclarator.ParameterList.Parameters.Select(parameter => parameter.Type.TypeName.Name).ToArray()))
            {
                context.Diagnostics.Add($"Duplicate function '{functionDeclarator.IdentifierName}'.");
            }

            adaptedFunctionDeclarator = null;
            return false;
        }
    }
}