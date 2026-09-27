using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptFunctionDeclaration(
            JassMapScriptAdapterContext context,
            JassFunctionDeclarationSyntax functionDeclaration,
            [NotNullWhen(true)] out JassTopLevelDeclarationSyntax? adaptedFunctionDeclaration)
        {
            foreach (var parameter in functionDeclaration.FunctionDeclarator.ParameterList.GetParameters())
            {
                if (!context.KnownTypes.ContainsKey(parameter.Type.GetToken().Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionDeclarationUnknownParameterVariableType, parameter.Type, functionDeclaration.FunctionDeclarator.IdentifierName, parameter.IdentifierName);
                }

                if (!context.KnownLocalVariables.TryAdd(parameter.IdentifierName.Token.Text, parameter.Type.GetToken().Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionDeclarationConflictingParameterVariableName, parameter.IdentifierName, functionDeclaration.FunctionDeclarator.IdentifierName);
                }
            }

            if (TryAdaptFunctionDeclarator(context, functionDeclaration.FunctionDeclarator, out var adaptedFunctionDeclarator) |
                TryAdaptStatementList(context, functionDeclaration.Statements, out var adaptedBody))
            {
                context.KnownLocalVariables.Clear();

                adaptedFunctionDeclaration = JassSyntaxFactory.FunctionDeclaration(
                    adaptedFunctionDeclarator ?? functionDeclaration.FunctionDeclarator,
                    adaptedBody ?? functionDeclaration.Statements);

                return true;
            }

            context.KnownLocalVariables.Clear();

            adaptedFunctionDeclaration = null;
            return false;
        }
    }
}