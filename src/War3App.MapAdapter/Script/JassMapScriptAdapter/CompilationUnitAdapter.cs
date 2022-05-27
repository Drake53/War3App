using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        public bool TryAdaptCompilationUnit(JassMapScriptAdapterContext context, JassCompilationUnitSyntax compilationUnit, [NotNullWhen(true)] out JassCompilationUnitSyntax? adaptedCompilationUnit)
        {
            if (compilationUnit is null)
            {
                throw new ArgumentNullException(nameof(compilationUnit));
            }

            var isAdapted = false;

            var declarationsBuilder = ImmutableArray.CreateBuilder<ITopLevelDeclarationSyntax>();
            foreach (var declaration in compilationUnit.Declarations)
            {
                if (TryAdaptDeclaration(context, declaration, out var adaptedDeclaration))
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
                adaptedCompilationUnit = new JassCompilationUnitSyntax(declarationsBuilder.ToImmutable());
                return true;
            }

            adaptedCompilationUnit = null;
            return false;
        }
    }
}