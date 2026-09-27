using System;
using System.Diagnostics.CodeAnalysis;

using War3App.MapAdapter.Diagnostics;

using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public partial class JassMapScriptAdapter
    {
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
                if (string.Equals(invocationName, "BlzCreateItemWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocationName, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments, "CreateItem", 4, 0, 3);
                }
                else if (string.Equals(invocationName, "BlzCreateUnitWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocationName, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments, "CreateUnit", 6, 1, 5);
                }
                else if (string.Equals(invocationName, "BlzCreateDestructableWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocationName, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDestructable", 7, 0, 6);
                }
                else if (string.Equals(invocationName, "BlzCreateDestructableZWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocationName, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDestructableZ", 8, 0, 7);
                }
                else if (string.Equals(invocationName, "BlzCreateDeadDestructableWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocationName, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDeadDestructable", 7, 0, 6);
                }
                else if (string.Equals(invocationName, "BlzCreateDeadDestructableZWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocationName, invocationArguments, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDeadDestructableZ", 8, 0, 7);
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

        private bool TryAdaptBlzCreateInvocation(
            JassMapScriptAdapterContext context,
            string invocationName,
            JassArgumentListSyntax invocationArguments,
            [NotNullWhen(true)] out string? adaptedInvocationName,
            [NotNullWhen(true)] out JassArgumentListSyntax? adaptedInvocationArguments,
            string replacementFunctionName,
            int expectedArgumentCount,
            int typeIdArgumentIndex,
            int skinIdArgumentIndex)
        {
            if (invocationArguments.ArgumentList.Items.Length == expectedArgumentCount)
            {
                var typeId = invocationArguments.ArgumentList.Items[typeIdArgumentIndex];
                var skinId = invocationArguments.ArgumentList.Items[skinIdArgumentIndex];

                if (typeId.NullableEquivalentTo(skinId))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationBlzCreateWithSkinIdRemoved, invocationName, replacementFunctionName);
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationBlzCreateWithSkinIdDeleted, invocationName, replacementFunctionName, skinId, typeId);
                }

                var arguments = new JassExpressionSyntax[expectedArgumentCount - 1];
                for (var i = 0; i < expectedArgumentCount; i++)
                {
                    if (i == skinIdArgumentIndex)
                    {
                        continue;
                    }

                    arguments[i > skinIdArgumentIndex ? i - 1 : i] = invocationArguments.ArgumentList.Items[i];
                }

                adaptedInvocationName = replacementFunctionName;
                adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(arguments);
                return true;
            }
            else
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, invocationName, expectedArgumentCount, invocationArguments.ArgumentList.Items.Length);

                adaptedInvocationName = null;
                adaptedInvocationArguments = null;
                return false;
            }
        }
    }
}