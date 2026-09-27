using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptGlobalDeclarationList(
            JassMapScriptAdapterContext context,
            JassGlobalsDeclarationSyntax globalDeclarationList,
            [NotNullWhen(true)] out JassTopLevelDeclarationSyntax? adaptedGlobalDeclarationList)
        {
            var isAdapted = false;

            var declarationsBuilder = ImmutableArray.CreateBuilder<JassGlobalDeclarationSyntax>();
            foreach (var declaration in globalDeclarationList.GlobalDeclarations)
            {
                if (TryAdaptIGlobalDeclaration(context, declaration, out var adaptedDeclaration))
                {
                    declarationsBuilder.Add(adaptedDeclaration);
                    isAdapted = true;
                }
                else
                {
                    declarationsBuilder.Add(declaration);
                }
            }

            if (isAdapted)
            {
                adaptedGlobalDeclarationList = JassSyntaxFactory.GlobalsDeclaration(declarationsBuilder.ToImmutable());
                return true;
            }

            adaptedGlobalDeclarationList = null;
            return false;
        }
    }
}