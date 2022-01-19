using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptTypeDeclaration(JassMapScriptAdapterContext context, JassTypeDeclarationSyntax typeDeclaration, [NotNullWhen(true)] out IDeclarationSyntax? adaptedTypeDeclaration)
        {
            if (!context.KnownTypes.ContainsKey(typeDeclaration.BaseType.TypeName.Name))
            {
                context.Diagnostics.Add($"Unknown base type '{typeDeclaration.BaseType}'.");
            }

            context.KnownTypes.Add(typeDeclaration.IdentifierName.Name, typeDeclaration.BaseType.TypeName.Name);

            adaptedTypeDeclaration = null;
            return false;
        }
    }
}