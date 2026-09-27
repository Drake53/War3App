using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptVariableDeclarator(
            JassMapScriptAdapterContext context,
            bool isGlobalVariable,
            JassVariableOrArrayDeclaratorSyntax declarator,
            [NotNullWhen(true)] out JassVariableOrArrayDeclaratorSyntax? adaptedDeclarator)
        {
            if (!context.KnownTypes.ContainsKey(declarator.GetVariableType().GetToken().Text))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableDeclarationUnknownType, declarator.GetVariableType(), declarator.GetIdentifierName());
            }

            if (isGlobalVariable)
            {
                if (!context.KnownGlobalVariables.TryAdd(declarator.GetIdentifierName().Token.Text, declarator.GetVariableType().GetToken().Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableDeclarationConflictingVariableName, declarator.GetIdentifierName(), "global");
                }
            }
            else
            {
                if (!context.KnownLocalVariables.TryAdd(declarator.GetIdentifierName().Token.Text, declarator.GetVariableType().GetToken().Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableDeclarationConflictingVariableName, declarator.GetIdentifierName(), "local");
                }
            }

            return declarator switch
            {
                JassArrayDeclaratorSyntax arrayDeclarator => TryAdaptArrayDeclarator(context, isGlobalVariable, arrayDeclarator, out adaptedDeclarator),
                JassVariableDeclaratorSyntax variableDeclarator => TryAdaptVariableDeclarator(context, isGlobalVariable, variableDeclarator, out adaptedDeclarator),

                _ => TryAdaptDummy(context, declarator, out adaptedDeclarator),
            };
        }

        private bool TryAdaptVariableDeclarator(
            JassMapScriptAdapterContext context,
            bool isGlobalVariable,
            JassVariableDeclaratorSyntax variableDeclarator,
            [NotNullWhen(true)] out JassVariableOrArrayDeclaratorSyntax? adaptedVariableDeclarator)
        {
            if (TryAdaptEqualsValueClause(context, variableDeclarator.Value, out var adaptedValue))
            {
                adaptedVariableDeclarator = JassSyntaxFactory.VariableDeclarator(
                    variableDeclarator.Type,
                    variableDeclarator.IdentifierName,
                    adaptedValue);

                return true;
            }

            adaptedVariableDeclarator = null;
            return false;
        }
    }
}