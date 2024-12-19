using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptTypeDeclaration(JassMapScriptAdapterContext context, JassTypeDeclarationSyntax typeDeclaration, [NotNullWhen(true)] out ITopLevelDeclarationSyntax? adaptedTypeDeclaration)
        {
            if (!context.KnownTypes.ContainsKey(typeDeclaration.BaseType.TypeName.Name))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.TypeDeclarationUnknownBaseType, typeDeclaration.BaseType, typeDeclaration.IdentifierName);
            }

            context.KnownTypes.Add(typeDeclaration.IdentifierName.Name, typeDeclaration.BaseType.TypeName.Name);

            adaptedTypeDeclaration = null;
            return false;
        }
    }
}