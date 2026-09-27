using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptArgumentList(
            JassMapScriptAdapterContext context,
            JassArgumentListSyntax argumentList,
            [NotNullWhen(true)] out JassArgumentListSyntax? adaptedArgumentList)
        {
            var isAdapted = false;

            var argumentsBuilder = ImmutableArray.CreateBuilder<JassExpressionSyntax>();
            foreach (var argument in argumentList.ArgumentList.Items)
            {
                if (TryAdaptExpression(context, argument, out var adaptedArgument))
                {
                    argumentsBuilder.Add(adaptedArgument);
                    isAdapted = true;
                }
                else
                {
                    argumentsBuilder.Add(argument);
                }
            }

            if (isAdapted)
            {
                adaptedArgumentList = JassSyntaxFactory.ArgumentList(argumentsBuilder.ToImmutable());
                return true;
            }

            adaptedArgumentList = null;
            return false;
        }
    }
}