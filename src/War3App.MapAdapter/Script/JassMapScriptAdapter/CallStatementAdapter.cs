using System.Diagnostics.CodeAnalysis;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptCallStatement(JassMapScriptAdapterContext context, JassCallStatementSyntax callStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedCallStatement)
        {
            if (context.KnownFunctions.TryGetValue(callStatement.IdentifierName.Name, out var knownFunctionParameters))
            {
                if (knownFunctionParameters.Length != callStatement.Arguments.Arguments.Length)
                {
                    context.Diagnostics.Add($"Invalid function invocation: '{callStatement.IdentifierName}' expected {knownFunctionParameters.Length} parameters but got {callStatement.Arguments.Arguments.Length}.");
                }
            }
            else
            {
                context.Diagnostics.Add($"Unknown function '{callStatement.IdentifierName}'.");

                adaptedCallStatement = new JassCommentStatementSyntax(callStatement.ToString());
                return true;
            }

            if (TryAdaptArgumentList(context, callStatement.Arguments, out var adaptedArguments))
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