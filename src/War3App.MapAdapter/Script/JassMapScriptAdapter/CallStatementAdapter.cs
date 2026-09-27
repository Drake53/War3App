using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptCallStatement(
            JassMapScriptAdapterContext context,
            JassCallStatementSyntax callStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedCallStatement)
        {
            if (TryAdaptInvocation(context, callStatement.IdentifierName.Token.Text, callStatement.ArgumentList, out var adaptedInvocationName, out var adaptedInvocationArguments))
            {
                if (string.IsNullOrEmpty(adaptedInvocationName))
                {
                    adaptedCallStatement = JassSyntaxFactory.CallStatement("DoNothing")
                        .WithTrailingTrivia(JassSyntaxFactory.SyntaxTriviaList(
                            JassSyntaxFactory.SingleLineCommentTrivia($"//{callStatement.ToFullString()}")));
                }
                else if (TryAdaptArgumentList(context, adaptedInvocationArguments, out var adaptedArguments))
                {
                    adaptedCallStatement = JassSyntaxFactory.CallStatement(
                        adaptedInvocationName,
                        adaptedArguments);
                }
                else
                {
                    adaptedCallStatement = JassSyntaxFactory.CallStatement(
                        adaptedInvocationName,
                        adaptedInvocationArguments);
                }

                return true;
            }
            else if (TryAdaptArgumentList(context, callStatement.ArgumentList, out var adaptedArguments))
            {
                adaptedCallStatement = JassSyntaxFactory.CallStatement(
                    callStatement.IdentifierName,
                    adaptedArguments);

                return true;
            }

            adaptedCallStatement = null;
            return false;
        }
    }
}