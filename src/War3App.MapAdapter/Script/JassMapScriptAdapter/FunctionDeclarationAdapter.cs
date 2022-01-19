using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptFunctionDeclaration(JassMapScriptAdapterContext context, JassFunctionDeclarationSyntax functionDeclaration, [NotNullWhen(true)] out IDeclarationSyntax? adaptedFunctionDeclaration)
        {
            foreach (var parameter in functionDeclaration.FunctionDeclarator.ParameterList.Parameters)
            {
                if (!context.KnownTypes.ContainsKey(parameter.Type.TypeName.Name))
                {
                    context.Diagnostics.Add($"Unknown variable type '{parameter.Type}'.");
                }

                if (!context.KnownLocalVariables.TryAdd(parameter.IdentifierName.Name, parameter.Type.TypeName.Name))
                {
                    context.Diagnostics.Add($"Duplicate local variable '{parameter.IdentifierName}'.");
                }
            }

            if (TryAdaptFunctionDeclarator(context, functionDeclaration.FunctionDeclarator, out var adaptedFunctionDeclarator) |
                TryAdaptStatementList(context, functionDeclaration.Body, out var adaptedBody))
            {
                context.KnownLocalVariables.Clear();

                adaptedFunctionDeclaration = new JassFunctionDeclarationSyntax(
                    adaptedFunctionDeclarator ?? functionDeclaration.FunctionDeclarator,
                    adaptedBody ?? functionDeclaration.Body);

                return true;
            }

            context.KnownLocalVariables.Clear();

            adaptedFunctionDeclaration = null;
            return false;
        }
    }
}