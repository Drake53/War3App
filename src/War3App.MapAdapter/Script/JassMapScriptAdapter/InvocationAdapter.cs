using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
        private const string FallbackNativeFunctionDeclarationsResourceName = "vendor/common.j";

        private static readonly Dictionary<string, string> FallbackFunctionNames = new(StringComparer.Ordinal)
        {
            ["BlzCreateItemWithSkin"] = "CreateItem",
            ["BlzCreateUnitWithSkin"] = "CreateUnit",

            ["BlzCreateDestructableWithSkin"] = "CreateDestructable",
            ["BlzCreateDestructablePitchRoll"] = "CreateDestructable",
            ["BlzCreateDestructableWithColor"] = "CreateDestructable",
            ["BlzCreateDestructableWithSkinPitchRoll"] = "BlzCreateDestructableWithSkin",
            ["BlzCreateDestructableWithSkinColor"] = "BlzCreateDestructableWithSkin",
            ["BlzCreateDestructablePitchRollWithColor"] = "BlzCreateDestructablePitchRoll",
            ["BlzCreateDestructableWithSkinPitchRollColor"] = "BlzCreateDestructableWithSkinPitchRoll",

            ["BlzCreateDestructableZWithSkin"] = "CreateDestructableZ",
            ["BlzCreateDestructableZPitchRoll"] = "CreateDestructableZ",
            ["BlzCreateDestructableZWithColor"] = "CreateDestructableZ",
            ["BlzCreateDestructableZWithSkinPitchRoll"] = "BlzCreateDestructableZWithSkin",
            ["BlzCreateDestructableZWithSkinColor"] = "BlzCreateDestructableZWithSkin",
            ["BlzCreateDestructableZPitchRollWithColor"] = "BlzCreateDestructableZPitchRoll",
            ["BlzCreateDestructableZWithSkinPitchRollColor"] = "BlzCreateDestructableZWithSkinPitchRoll",

            ["BlzCreateDeadDestructableWithSkin"] = "CreateDeadDestructable",
            ["BlzCreateDeadDestructablePitchRoll"] = "CreateDeadDestructable",
            ["BlzCreateDeadDestructableWithColor"] = "CreateDeadDestructable",
            ["BlzCreateDeadDestructableWithSkinPitchRoll"] = "BlzCreateDeadDestructableWithSkin",
            ["BlzCreateDeadDestructableWithSkinColor"] = "BlzCreateDeadDestructableWithSkin",
            ["BlzCreateDeadDestructablePitchRollWithColor"] = "BlzCreateDeadDestructablePitchRoll",
            ["BlzCreateDeadDestructableWithSkinPitchRollColor"] = "BlzCreateDeadDestructableWithSkinPitchRoll",

            ["BlzCreateDeadDestructableZWithSkin"] = "CreateDeadDestructableZ",
            ["BlzCreateDeadDestructableZPitchRoll"] = "CreateDeadDestructableZ",
            ["BlzCreateDeadDestructableZWithColor"] = "CreateDeadDestructableZ",
            ["BlzCreateDeadDestructableZWithSkinPitchRoll"] = "BlzCreateDeadDestructableZWithSkin",
            ["BlzCreateDeadDestructableZWithSkinColor"] = "BlzCreateDeadDestructableZWithSkin",
            ["BlzCreateDeadDestructableZPitchRollWithColor"] = "BlzCreateDeadDestructableZPitchRoll",
            ["BlzCreateDeadDestructableZWithSkinPitchRollColor"] = "BlzCreateDeadDestructableZWithSkinPitchRoll",
        };

        private static readonly Dictionary<string, JassNativeFunctionDeclarationSyntax> FallbackNativeFunctionDeclarations = LoadFallbackNativeFunctionDeclarations();

        private static readonly HashSet<string> TypeIdParameterNames = new(StringComparer.Ordinal)
        {
            "itemid",
            "unitid",
            "objectid",
        };

        private bool TryAdaptInvocation(
            JassMapScriptAdapterContext context,
            string invocationName,
            JassArgumentListSyntax invocationArguments,
            out string? adaptedInvocationName,
            [NotNullWhen(true)] out JassArgumentListSyntax? adaptedInvocationArguments)
        {
            if (context.KnownFunctions.TryGetValue(invocationName, out var knownFunctionParameters))
            {
                if (knownFunctionParameters.Length != invocationArguments.ArgumentList.Items.Length)
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, invocationName, knownFunctionParameters.Length, invocationArguments.ArgumentList.Items.Length);
                }
            }
            else
            {
                if (FallbackNativeFunctionDeclarations.TryGetValue(invocationName, out var nativeFunctionDeclaration))
                {
                    return TryAdaptFallbackInvocation(context, nativeFunctionDeclaration, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments);
                }
                else if (string.Equals(invocationName, "SetSoundFacialAnimationLabel", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else if (string.Equals(invocationName, "SetSoundFacialAnimationGroupLabel", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else if (string.Equals(invocationName, "SetSoundFacialAnimationSetFilepath", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else if (string.Equals(invocationName, "SetDialogueSpeakerNameKey", StringComparison.Ordinal))
                {
                    if (invocationArguments.ArgumentList.Items.Length == 2 &&
                        invocationArguments.ArgumentList.Items[0] is JassIdentifierNameSyntax identifierName &&
                        invocationArguments.ArgumentList.Items[1].TryGetStringExpressionValue(out var stringValue))
                    {
                        if (!context.DialogueTitles.TryAdd(identifierName.Token.Text, stringValue))
                        {
                            context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationConflictingDialogueTitle, stringValue, identifierName);
                        }
                    }

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else if (string.Equals(invocationName, "SetDialogueTextKey", StringComparison.Ordinal))
                {
                    if (invocationArguments.ArgumentList.Items.Length == 2 &&
                        invocationArguments.ArgumentList.Items[0] is JassIdentifierNameSyntax identifierName &&
                        invocationArguments.ArgumentList.Items[1].TryGetStringExpressionValue(out var stringValue))
                    {
                        if (!context.DialogueTexts.TryAdd(identifierName.Token.Text, stringValue))
                        {
                            context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationConflictingDialogueText, stringValue, identifierName);
                        }
                    }

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else if (string.Equals(invocationName, "PlayDialogueFromSpeakerEx", StringComparison.Ordinal))
                {
                    if (invocationArguments.ArgumentList.Items.Length == 7 &&
                        invocationArguments.ArgumentList.Items[3] is JassIdentifierNameSyntax identifierName &&
                        context.DialogueTitles.TryGetValue(identifierName.Token.Text, out var dialogueTitle) &&
                        context.DialogueTexts.TryGetValue(identifierName.Token.Text, out var dialogueText))
                    {
                        adaptedInvocationName = "TransmissionFromUnitWithNameBJ";
                        adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(
                            invocationArguments.ArgumentList.Items[0],
                            invocationArguments.ArgumentList.Items[1],
                            JassSyntaxFactory.LiteralExpression(JassSyntaxFactory.Literal(dialogueTitle)),
                            invocationArguments.ArgumentList.Items[3],
                            JassSyntaxFactory.LiteralExpression(JassSyntaxFactory.Literal(dialogueText)),
                            invocationArguments.ArgumentList.Items[4],
                            invocationArguments.ArgumentList.Items[5],
                            invocationArguments.ArgumentList.Items[6]);

                        return true;
                    }
                }
                else if (string.Equals(invocationName, "PlayDialogueFromSpeakerTypeEx", StringComparison.Ordinal))
                {
                    if (invocationArguments.ArgumentList.Items.Length == 8 &&
                        invocationArguments.ArgumentList.Items[4] is JassIdentifierNameSyntax identifierName &&
                        context.DialogueTitles.TryGetValue(identifierName.Token.Text, out var dialogueTitle) &&
                        context.DialogueTexts.TryGetValue(identifierName.Token.Text, out var dialogueText))
                    {
                        adaptedInvocationName = "TransmissionFromUnitTypeWithNameBJ";
                        adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(
                            invocationArguments.ArgumentList.Items[0],
                            invocationArguments.ArgumentList.Items[1],
                            invocationArguments.ArgumentList.Items[2],
                            JassSyntaxFactory.LiteralExpression(JassSyntaxFactory.Literal(dialogueTitle)),
                            invocationArguments.ArgumentList.Items[3],
                            invocationArguments.ArgumentList.Items[4],
                            JassSyntaxFactory.LiteralExpression(JassSyntaxFactory.Literal(dialogueText)),
                            invocationArguments.ArgumentList.Items[5],
                            invocationArguments.ArgumentList.Items[6],
                            invocationArguments.ArgumentList.Items[7]);

                        return true;
                    }
                }
                else if (string.Equals(invocationName, "SetEnemyStartLocPrioCount", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else if (string.Equals(invocationName, "SetEnemyStartLocPrio", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocationArguments;
                    return true;
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationUnknownFunctionIdentifier, invocationName, "call statement or invocation expression");

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = null;
                    return false;
                }
            }

            adaptedInvocationName = null;
            adaptedInvocationArguments = null;
            return false;
        }

        private bool TryAdaptFallbackInvocation(
            JassMapScriptAdapterContext context,
            JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration,
            JassArgumentListSyntax invocationArguments,
            [NotNullWhen(true)] out string? adaptedInvocationName,
            [NotNullWhen(true)] out JassArgumentListSyntax? adaptedInvocationArguments)
        {
            var invocationName = nativeFunctionDeclaration.IdentifierName.Token.Text;
            var parameters = nativeFunctionDeclaration.ParameterList.GetParameters().ToArray();
            var arguments = invocationArguments.ArgumentList.Items;
            if (arguments.Length != parameters.Length)
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, invocationName, parameters.Length, arguments.Length);

                adaptedInvocationName = null;
                adaptedInvocationArguments = null;
                return false;
            }

            var fallbackNativeFunctionDeclaration = nativeFunctionDeclaration;
            do
            {
                if (!FallbackFunctionNames.TryGetValue(fallbackNativeFunctionDeclaration.IdentifierName.Token.Text, out var nextFallbackFunctionName) ||
                    !FallbackNativeFunctionDeclarations.TryGetValue(nextFallbackFunctionName, out fallbackNativeFunctionDeclaration))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationUnknownFunctionIdentifier, invocationName, "call statement or invocation expression");

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = null;
                    return false;
                }
            }
            while (!context.KnownFunctions.ContainsKey(fallbackNativeFunctionDeclaration.IdentifierName.Token.Text));

            var fallbackFunctionName = fallbackNativeFunctionDeclaration.IdentifierName.Token.Text;

            var argumentsByParameterName = new Dictionary<string, JassExpressionSyntax>(StringComparer.Ordinal);
            for (var i = 0; i < parameters.Length; i++)
            {
                argumentsByParameterName.Add(parameters[i].IdentifierName.Token.Text, arguments[i]);
            }

            var fallbackParameterNames = fallbackNativeFunctionDeclaration.ParameterList.GetParameters()
                .Select(parameter => parameter.IdentifierName.Token.Text)
                .ToArray();

            var adaptedArguments = fallbackParameterNames
                .Select(parameterName => argumentsByParameterName[parameterName])
                .ToArray();

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterName = parameters[i].IdentifierName.Token.Text;
                if (fallbackParameterNames.Contains(parameterName, StringComparer.Ordinal))
                {
                    continue;
                }

                var argument = arguments[i];
                if (string.Equals(parameterName, "skinId", StringComparison.Ordinal) &&
                    argumentsByParameterName.Any(parameterArgument => TypeIdParameterNames.Contains(parameterArgument.Key) && parameterArgument.Value.NullableEquivalentTo(argument)))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterRemoved, invocationName, fallbackFunctionName, parameterName);
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterDeleted, invocationName, fallbackFunctionName, parameterName, argument);
                }
            }

            adaptedInvocationName = fallbackFunctionName;
            adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(adaptedArguments);
            return true;
        }

        private static Dictionary<string, JassNativeFunctionDeclarationSyntax> LoadFallbackNativeFunctionDeclarations()
        {
            using var stream = typeof(JassMapScriptAdapter).Assembly.GetManifestResourceStream(FallbackNativeFunctionDeclarationsResourceName);
            if (stream is null)
            {
                throw new InvalidOperationException($"Embedded resource '{FallbackNativeFunctionDeclarationsResourceName}' not found.");
            }

            using var reader = new StreamReader(stream);
            var compilationUnit = JassSyntaxFactory.ParseCompilationUnit(reader.ReadToEnd());

            var nativeFunctionDeclarations = new Dictionary<string, JassNativeFunctionDeclarationSyntax>(StringComparer.Ordinal);
            foreach (var declaration in compilationUnit.Declarations)
            {
                if (declaration is JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration)
                {
                    nativeFunctionDeclarations.Add(nativeFunctionDeclaration.IdentifierName.Token.Text, nativeFunctionDeclaration);
                }
            }

            return nativeFunctionDeclarations;
        }
    }
}