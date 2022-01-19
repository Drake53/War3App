using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptInvocationExpression(JassMapScriptAdapterContext context, JassInvocationExpressionSyntax invocationExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedInvocationExpression)
        {
            if (context.KnownFunctions.TryGetValue(invocationExpression.IdentifierName.Name, out var knownFunctionParameters))
            {
                if (knownFunctionParameters.Length != invocationExpression.Arguments.Arguments.Length)
                {
                    context.Diagnostics.Add($"Invalid function invocation: '{invocationExpression.IdentifierName}' expected {knownFunctionParameters.Length} parameters but got {invocationExpression.Arguments.Arguments.Length}.");
                }
            }
            else
            {
                context.Diagnostics.Add($"Unknown function '{invocationExpression.IdentifierName}'.");
            }

            if (TryAdaptArgumentList(context, invocationExpression.Arguments, out var adaptedArguments))
            {
                adaptedInvocationExpression = new JassInvocationExpressionSyntax(
                    invocationExpression.IdentifierName,
                    adaptedArguments);

                return true;
            }

            adaptedInvocationExpression = null;
            return false;
        }
    }
}