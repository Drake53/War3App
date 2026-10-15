using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
                    var originalLeadingTrivia = callStatement.GetLeadingTrivia();
                    var originalTrailingTrivia = callStatement.GetTrailingTrivia();
                    var strippedCallStatement = callStatement
                        .WithLeadingTrivia(JassSyntaxTriviaList.Empty)
                        .WithTrailingTrivia(JassSyntaxTriviaList.Empty)
                        .ToFullString()
                        .Replace("\r", "\\r", StringComparison.Ordinal)
                        .Replace("\n", "\\n", StringComparison.Ordinal);

                    var callStatementAsComment = JassSyntaxFactory.SingleLineCommentTrivia($"//{strippedCallStatement}");
                    var trailingTrivia = JassSyntaxFactory.SyntaxTriviaList(
                        originalTrailingTrivia.Trivia.Prepend(callStatementAsComment));

                    adaptedCallStatement = JassSyntaxFactory.CallStatement(
                        JassSyntaxFactory.Token(JassSyntaxKind.CallKeyword, JassSyntaxTriviaList.SingleSpace),
                        JassSyntaxFactory.IdentifierName("DoNothing"),
                        JassArgumentListSyntax.Empty)
                        .WithLeadingTrivia(originalLeadingTrivia)
                        .WithTrailingTrivia(trailingTrivia);
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