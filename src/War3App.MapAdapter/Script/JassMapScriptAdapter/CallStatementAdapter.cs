using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptCallStatement(JassMapScriptAdapterContext context, JassCallStatementSyntax callStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedCallStatement)
        {
            if (TryAdaptInvocation(context, callStatement, out var adaptedInvocationName, out var adaptedInvocationArguments))
            {
                if (string.IsNullOrEmpty(adaptedInvocationName))
                {
                    adaptedCallStatement = new JassCommentStatementSyntax(callStatement.ToString());
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
            else if (TryAdaptArgumentList(context, callStatement.Arguments, out var adaptedArguments))
            {
                adaptedCallStatement = new JassCallStatementSyntax(
                    callStatement.IdentifierName,
                    adaptedArguments);

                return true;
            }

            adaptedCallStatement = null;
            return false;
        }
    }
}