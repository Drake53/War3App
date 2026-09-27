using System.Diagnostics.CodeAnalysis;
using System.Linq;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptFunctionDeclarator(
            JassMapScriptAdapterContext context,
            JassFunctionDeclaratorSyntax functionDeclarator,
            [NotNullWhen(true)] out JassFunctionDeclaratorSyntax? adaptedFunctionDeclarator)
        {
            if (!context.KnownFunctions.TryAdd(
                functionDeclarator.IdentifierName.Token.Text,
                functionDeclarator.ParameterList.GetParameters().Select(parameter => parameter.Type.GetToken().Text).ToArray()))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionDeclarationConflictingFunctionName, functionDeclarator.IdentifierName);
            }

            adaptedFunctionDeclarator = null;
            return false;
        }
    }
}