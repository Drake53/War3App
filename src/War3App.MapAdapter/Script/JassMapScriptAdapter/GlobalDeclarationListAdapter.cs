using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptGlobalDeclarationList(JassMapScriptAdapterContext context, JassGlobalDeclarationListSyntax globalDeclarationList, [NotNullWhen(true)] out ITopLevelDeclarationSyntax? adaptedGlobalDeclarationList)
        {
            var isAdapted = false;

            var declarationsBuilder = ImmutableArray.CreateBuilder<IGlobalDeclarationSyntax>();
            foreach (var declaration in globalDeclarationList.Globals)
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
                adaptedGlobalDeclarationList = new JassGlobalDeclarationListSyntax(declarationsBuilder.ToImmutable());
                return true;
            }

            adaptedGlobalDeclarationList = null;
            return false;
        }
    }
}