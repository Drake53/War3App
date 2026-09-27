using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptInvocationExpression(
            JassMapScriptAdapterContext context,
            JassInvocationExpressionSyntax invocationExpression,
            [NotNullWhen(true)] out JassExpressionSyntax? adaptedInvocationExpression)
        {
            if (TryAdaptInvocation(context, invocationExpression.IdentifierName.Token.Text, invocationExpression.ArgumentList, out var adaptedInvocationName, out var adaptedInvocationArguments))
            {
                if (string.IsNullOrEmpty(adaptedInvocationName))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationExpressionShouldBeRemoved, invocationExpression.IdentifierName);
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
            else if (TryAdaptArgumentList(context, invocationExpression.ArgumentList, out var adaptedArguments))
            {
                adaptedInvocationExpression = JassSyntaxFactory.InvocationExpression(
                    invocationExpression.IdentifierName,
                    adaptedArguments);

                return true;
            }

            adaptedInvocationExpression = null;
            return false;
        }
    }
}