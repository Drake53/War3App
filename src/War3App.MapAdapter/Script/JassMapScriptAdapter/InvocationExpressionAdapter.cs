using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptInvocationExpression(JassMapScriptAdapterContext context, JassInvocationExpressionSyntax invocationExpression, [NotNullWhen(true)] out IExpressionSyntax? adaptedInvocationExpression)
        {
            if (TryAdaptInvocation(context, invocationExpression, out var adaptedInvocationName, out var adaptedInvocationArguments))
            {
                if (string.IsNullOrEmpty(adaptedInvocationName))
                {
                    context.Diagnostics.Add($"Invocation expression of function '{invocationExpression.IdentifierName}' should be removed.");
                    adaptedInvocationExpression = invocationExpression;
                }
                else if (TryAdaptArgumentList(context, adaptedInvocationArguments, out var adaptedArguments))
                {
                    adaptedInvocationExpression = JassSyntaxFactory.InvocationExpression(
                        adaptedInvocationName,
                        adaptedArguments);
                }
                else
                {
                    adaptedInvocationExpression = JassSyntaxFactory.InvocationExpression(
                        adaptedInvocationName,
                        adaptedInvocationArguments);
                }

                return true;
            }
            else if (TryAdaptArgumentList(context, invocationExpression.Arguments, out var adaptedArguments))
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