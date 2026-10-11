using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptSetStatement(
            JassMapScriptAdapterContext context,
            JassSetStatementSyntax setStatement,
            [NotNullWhen(true)] out JassStatementSyntax? adaptedSetStatement)
        {
            if (!context.KnownLocalVariables.ContainsKey(setStatement.IdentifierName.Token.Text) &&
                !context.KnownGlobalVariables.ContainsKey(setStatement.IdentifierName.Token.Text))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.SetStatementUnknownVariableIdentifier, setStatement.IdentifierName);
            }

            if (TryAdaptElementAccessClause(context, setStatement.ElementAccessClause, out var adaptedElementAccessClause) |
                TryAdaptEqualsValueClause(context, setStatement.Value, out var adaptedValue))
            {
                adaptedSetStatement = JassSyntaxFactory.SetStatement(
                    setStatement.IdentifierName,
                    adaptedElementAccessClause ?? setStatement.ElementAccessClause,
                    adaptedValue ?? setStatement.Value);

                return true;
            }

            adaptedSetStatement = null;
            return false;
        }
    }
}