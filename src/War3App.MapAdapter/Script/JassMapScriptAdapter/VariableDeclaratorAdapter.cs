using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptVariableDeclarator(JassMapScriptAdapterContext context, bool isGlobalVariable, IVariableDeclaratorSyntax declarator, [NotNullWhen(true)] out IVariableDeclaratorSyntax? adaptedDeclarator)
        {
            if (!context.KnownTypes.ContainsKey(declarator.Type.TypeName.Name))
            {
                context.Diagnostics.Add($"Unknown variable type '{declarator.Type}'.");
            }

            if (isGlobalVariable)
            {
                if (!context.KnownGlobalVariables.TryAdd(declarator.IdentifierName.Name, declarator.Type.TypeName.Name))
                {
                    context.Diagnostics.Add($"Duplicate global variable '{declarator.IdentifierName}'.");
                }
            }
            else
            {
                if (!context.KnownLocalVariables.TryAdd(declarator.IdentifierName.Name, declarator.Type.TypeName.Name))
                {
                    context.Diagnostics.Add($"Duplicate local variable '{declarator.IdentifierName}'.");
                }
            }

            return declarator switch
            {
                JassArrayDeclaratorSyntax arrayDeclarator => TryAdaptArrayDeclarator(context, isGlobalVariable, arrayDeclarator, out adaptedDeclarator),
                JassVariableDeclaratorSyntax variableDeclarator => TryAdaptVariableDeclarator(context, isGlobalVariable, variableDeclarator, out adaptedDeclarator),

                _ => TryAdaptDummy(context, declarator, out adaptedDeclarator),
            };
        }

        private bool TryAdaptVariableDeclarator(JassMapScriptAdapterContext context, bool isGlobalVariable, JassVariableDeclaratorSyntax variableDeclarator, [NotNullWhen(true)] out IVariableDeclaratorSyntax? adaptedVariableDeclarator)
        {
            if (TryAdaptEqualsValueClause(context, variableDeclarator.Value, out var adaptedValue))
            {
                adaptedVariableDeclarator = new JassVariableDeclaratorSyntax(
                    variableDeclarator.Type,
                    variableDeclarator.IdentifierName,
                    adaptedValue);

                return true;
            }

            adaptedVariableDeclarator = null;
            return false;
        }
    }
}