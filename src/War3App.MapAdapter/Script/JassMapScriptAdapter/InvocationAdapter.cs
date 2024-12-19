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
        private bool TryAdaptInvocation<TInvocation>(JassMapScriptAdapterContext context, TInvocation invocation, out string? adaptedInvocationName, [NotNullWhen(true)] out JassArgumentListSyntax? adaptedInvocationArguments)
            where TInvocation : IInvocationSyntax
        {
            if (context.KnownFunctions.TryGetValue(invocation.IdentifierName.Name, out var knownFunctionParameters))
            {
                if (knownFunctionParameters.Length != invocation.Arguments.Arguments.Length)
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, invocation.IdentifierName, knownFunctionParameters.Length, invocation.Arguments.Arguments.Length);
                }
            }
            else
            {
                if (string.Equals(invocation.IdentifierName.Name, "BlzCreateItemWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocation, out adaptedInvocationName, out adaptedInvocationArguments, "CreateItem", 4, 0, 3);
                }
                else if (string.Equals(invocation.IdentifierName.Name, "BlzCreateUnitWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocation, out adaptedInvocationName, out adaptedInvocationArguments, "CreateUnit", 6, 1, 5);
                }
                else if (string.Equals(invocation.IdentifierName.Name, "BlzCreateDestructableWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocation, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDestructable", 7, 0, 6);
                }
                else if (string.Equals(invocation.IdentifierName.Name, "BlzCreateDestructableZWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocation, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDestructableZ", 8, 0, 7);
                }
                else if (string.Equals(invocation.IdentifierName.Name, "BlzCreateDeadDestructableWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocation, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDeadDestructable", 7, 0, 6);
                }
                else if (string.Equals(invocation.IdentifierName.Name, "BlzCreateDeadDestructableZWithSkin", StringComparison.Ordinal))
                {
                    return TryAdaptBlzCreateInvocation(context, invocation, out adaptedInvocationName, out adaptedInvocationArguments, "CreateDeadDestructableZ", 8, 0, 7);
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetSoundFacialAnimationLabel", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetSoundFacialAnimationGroupLabel", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetSoundFacialAnimationSetFilepath", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetDialogueSpeakerNameKey", StringComparison.Ordinal))
                {
                    if (invocation.Arguments.Arguments.Length == 2 &&
                        invocation.Arguments.Arguments[0] is JassVariableReferenceExpressionSyntax variableReferenceExpression &&
                        invocation.Arguments.Arguments[1] is JassStringLiteralExpressionSyntax stringLiteralExpression)
                    {
                        if (!context.DialogueTitles.TryAdd(variableReferenceExpression.IdentifierName.Name, stringLiteralExpression.Value))
                        {
                            context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationConflictingDialogueTitle, stringLiteralExpression.Value, variableReferenceExpression.IdentifierName);
                        }
                    }

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetDialogueTextKey", StringComparison.Ordinal))
                {
                    if (invocation.Arguments.Arguments.Length == 2 &&
                        invocation.Arguments.Arguments[0] is JassVariableReferenceExpressionSyntax variableReferenceExpression &&
                        invocation.Arguments.Arguments[1] is JassStringLiteralExpressionSyntax stringLiteralExpression)
                    {
                        if (!context.DialogueTexts.TryAdd(variableReferenceExpression.IdentifierName.Name, stringLiteralExpression.Value))
                        {
                            context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationConflictingDialogueText, stringLiteralExpression.Value, variableReferenceExpression.IdentifierName);
                        }
                    }

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else if (string.Equals(invocation.IdentifierName.Name, "PlayDialogueFromSpeakerEx", StringComparison.Ordinal))
                {
                    if (invocation.Arguments.Arguments.Length == 7 &&
                        invocation.Arguments.Arguments[3] is JassVariableReferenceExpressionSyntax variableReferenceExpression &&
                        context.DialogueTitles.TryGetValue(variableReferenceExpression.IdentifierName.Name, out var dialogueTitle) &&
                        context.DialogueTexts.TryGetValue(variableReferenceExpression.IdentifierName.Name, out var dialogueText))
                    {
                        adaptedInvocationName = "TransmissionFromUnitWithNameBJ";
                        adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(
                            invocation.Arguments.Arguments[0],
                            invocation.Arguments.Arguments[1],
                            JassSyntaxFactory.LiteralExpression(dialogueTitle),
                            invocation.Arguments.Arguments[3],
                            JassSyntaxFactory.LiteralExpression(dialogueText),
                            invocation.Arguments.Arguments[4],
                            invocation.Arguments.Arguments[5],
                            invocation.Arguments.Arguments[6]);

                        return true;
                    }
                }
                else if (string.Equals(invocation.IdentifierName.Name, "PlayDialogueFromSpeakerTypeEx", StringComparison.Ordinal))
                {
                    if (invocation.Arguments.Arguments.Length == 8 &&
                        invocation.Arguments.Arguments[4] is JassVariableReferenceExpressionSyntax variableReferenceExpression &&
                        context.DialogueTitles.TryGetValue(variableReferenceExpression.IdentifierName.Name, out var dialogueTitle) &&
                        context.DialogueTexts.TryGetValue(variableReferenceExpression.IdentifierName.Name, out var dialogueText))
                    {
                        adaptedInvocationName = "TransmissionFromUnitTypeWithNameBJ";
                        adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(
                            invocation.Arguments.Arguments[0],
                            invocation.Arguments.Arguments[1],
                            invocation.Arguments.Arguments[2],
                            JassSyntaxFactory.LiteralExpression(dialogueTitle),
                            invocation.Arguments.Arguments[3],
                            invocation.Arguments.Arguments[4],
                            JassSyntaxFactory.LiteralExpression(dialogueText),
                            invocation.Arguments.Arguments[5],
                            invocation.Arguments.Arguments[6],
                            invocation.Arguments.Arguments[7]);

                        return true;
                    }
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetEnemyStartLocPrioCount", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else if (string.Equals(invocation.IdentifierName.Name, "SetEnemyStartLocPrio", StringComparison.Ordinal))
                {
                    adaptedInvocationName = null;
                    adaptedInvocationArguments = invocation.Arguments;
                    return true;
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationUnknownFunctionIdentifier, invocation.IdentifierName, invocation is JassCallStatementSyntax ? "call statement" : "invocation expression");

                    adaptedInvocationName = null;
                    adaptedInvocationArguments = null;
                    return false;
                }
            }

            adaptedInvocationName = null;
            adaptedInvocationArguments = null;
            return false;
        }

        private bool TryAdaptBlzCreateInvocation<TInvocation>(
            JassMapScriptAdapterContext context,
            TInvocation invocation,
            [NotNullWhen(true)] out string? adaptedInvocationName,
            [NotNullWhen(true)] out JassArgumentListSyntax? adaptedInvocationArguments,
            string replacementFunctionName,
            int expectedArgumentCount,
            int typeIdArgumentIndex,
            int skinIdArgumentIndex)
            where TInvocation : IInvocationSyntax
        {
            if (invocation.Arguments.Arguments.Length == expectedArgumentCount)
            {
                var typeId = invocation.Arguments.Arguments[typeIdArgumentIndex];
                var skinId = invocation.Arguments.Arguments[skinIdArgumentIndex];

                if (typeId.NullableEquals(skinId))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationBlzCreateWithSkinIdRemoved, invocation.IdentifierName, replacementFunctionName);
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationBlzCreateWithSkinIdDeleted, invocation.IdentifierName, replacementFunctionName, skinId, typeId);
                }

                var arguments = new IExpressionSyntax[expectedArgumentCount - 1];
                for (var i = 0; i < expectedArgumentCount; i++)
                {
                    if (i == skinIdArgumentIndex)
                    {
                        continue;
                    }

                    arguments[i > skinIdArgumentIndex ? i - 1 : i] = invocation.Arguments.Arguments[i];
                }

                adaptedInvocationName = replacementFunctionName;
                adaptedInvocationArguments = JassSyntaxFactory.ArgumentList(arguments);
                return true;
            }
            else
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, invocation.IdentifierName, expectedArgumentCount, invocation.Arguments.Arguments.Length);

                adaptedInvocationName = null;
                adaptedInvocationArguments = null;
                return false;
            }
        }
    }
}