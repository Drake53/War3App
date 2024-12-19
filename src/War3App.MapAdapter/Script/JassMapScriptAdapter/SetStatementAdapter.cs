using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptSetStatement(JassMapScriptAdapterContext context, JassSetStatementSyntax setStatement, [NotNullWhen(true)] out IStatementSyntax? adaptedSetStatement)
        {
            if (!context.KnownLocalVariables.ContainsKey(setStatement.IdentifierName.Name) &&
                !context.KnownGlobalVariables.ContainsKey(setStatement.IdentifierName.Name))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.SetStatementUnknownVariableIdentifier, setStatement.IdentifierName);
            }

            if (TryAdaptExpression(context, setStatement.Indexer, out var adaptedIndexer) |
                TryAdaptEqualsValueClause(context, setStatement.Value, out var adaptedValue))
            {
                adaptedSetStatement = new JassSetStatementSyntax(
                    setStatement.IdentifierName,
                    adaptedIndexer ?? setStatement.Indexer,
                    adaptedValue ?? setStatement.Value);

                return true;
            }

            adaptedSetStatement = null;
            return false;
        }
    }
}