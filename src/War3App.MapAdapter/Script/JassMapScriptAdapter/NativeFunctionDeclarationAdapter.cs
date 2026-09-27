using System.Diagnostics.CodeAnalysis;
using System.Linq;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private bool TryAdaptNativeFunctionDeclaration(
            JassMapScriptAdapterContext context,
            JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration,
            [NotNullWhen(true)] out JassTopLevelDeclarationSyntax? adaptedNativeFunctionDeclaration)
        {
            if (!context.KnownFunctions.TryAdd(
                nativeFunctionDeclaration.IdentifierName.Token.Text,
                nativeFunctionDeclaration.ParameterList.GetParameters().Select(parameter => parameter.Type.GetToken().Text).ToArray()))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionDeclarationConflictingFunctionName, nativeFunctionDeclaration.IdentifierName);
            }

            adaptedNativeFunctionDeclaration = null;
            return false;
        }
    }
}