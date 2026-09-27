using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Extensions;
using War3Net.Build.Script;
using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Extensions;
using War3Net.CodeAnalysis.Jass.Syntax;
using War3Net.Common.Providers;

namespace War3App.MapAdapter.Script
{
    [Obsolete("Replace with visitor pattern")]
    public sealed partial class JassMapScriptAdapter : IMapFileAdapter
    {
        private static readonly JassMapScriptAdapter _instance = new();

        private JassMapScriptAdapter()
        {
        }

        public static JassMapScriptAdapter Instance => _instance;

        public string MapFileDescription => "Map Script (JASS)";

        public string DefaultFileName => JassMapScript.FileName;

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            var commonJStream = context.TargetPatch.OpenGameDataFile(PathConstants.CommonJPath);
            if (commonJStream is null)
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.CommonJPath);
            }

            var blizzardJStream = context.TargetPatch.OpenGameDataFile(PathConstants.BlizzardJPath);
            if (blizzardJStream is null)
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.BlizzardJPath);
            }

            if (commonJStream is null ||
                blizzardJStream is null)
            {
                return MapFileStatus.Inconclusive;
            }

            var commonJText = commonJStream.ReadAllText();
            var commonJCompilationUnit = JassSyntaxFactory.ParseCompilationUnit(commonJText);

            var blizzardJText = blizzardJStream.ReadAllText();
            var blizzardJCompilationUnit = JassSyntaxFactory.ParseCompilationUnit(blizzardJText);

            JassCompilationUnitSyntax compilationUnit;
            try
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                compilationUnit = JassSyntaxFactory.ParseCompilationUnit(reader.ReadToEnd());
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }

            var scriptAdapterContext = new JassMapScriptAdapterContext(context);

            foreach (var declaration in commonJCompilationUnit.Declarations)
            {
                RegisterDeclaration(declaration, scriptAdapterContext);
            }

            foreach (var declaration in blizzardJCompilationUnit.Declarations)
            {
                RegisterDeclaration(declaration, scriptAdapterContext);
            }

            // Common.j and Blizzard.j should not report any diagnostics.
            if (context.HasDiagnostics)
            {
                return MapFileStatus.Error;
            }

            var adapted = TryAdaptCompilationUnit(scriptAdapterContext, compilationUnit, out var adaptedCompilationUnit);

            var status = context.HasErrorDiagnostics
                ? MapFileStatus.Incompatible
                : MapFileStatus.Compatible;

            if (!adapted)
            {
                return status;
            }

            try
            {
                var memoryStream = new MemoryStream();

                using var writer = new StreamWriter(memoryStream, UTF8EncodingProvider.StrictUTF8, leaveOpen: true);
                var renderer = new JassRenderer(writer);
                renderer.Render(adaptedCompilationUnit);

                return AdaptResult.Create(memoryStream, status);
            }
            catch (Exception e)
            {
                return context.ReportSerializeError(e);
            }
        }

        private static void RegisterDeclaration(JassTopLevelDeclarationSyntax declaration, JassMapScriptAdapterContext context)
        {
            if (declaration is JassTypeDeclarationSyntax typeDeclaration)
            {
                if (!context.KnownTypes.ContainsKey(typeDeclaration.BaseType.GetToken().Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.TypeDeclarationUnknownBaseType, typeDeclaration.BaseType, typeDeclaration.IdentifierName);
                }

                context.KnownTypes.Add(typeDeclaration.IdentifierName.Token.Text, typeDeclaration.BaseType.GetToken().Text);
            }
            else if (declaration is JassGlobalsDeclarationSyntax globalDeclarationList)
            {
                foreach (var global in globalDeclarationList.GlobalDeclarations)
                {
                    if (global is JassGlobalVariableDeclarationSyntax globalVariableDeclaration)
                    {
                        RegisterVariableDeclarator(globalVariableDeclaration.Declarator, true, context);
                    }
                }
            }
            else if (declaration is JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration)
            {
                if (!context.KnownFunctions.TryAdd(
                    nativeFunctionDeclaration.IdentifierName.Token.Text,
                    nativeFunctionDeclaration.ParameterList.GetParameters().Select(parameter => parameter.Type.GetToken().Text).ToArray()))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionDeclarationConflictingFunctionName, nativeFunctionDeclaration.IdentifierName);
                }
            }
            else if (declaration is JassFunctionDeclarationSyntax functionDeclaration)
            {
                if (!context.KnownFunctions.TryAdd(
                    functionDeclaration.FunctionDeclarator.IdentifierName.Token.Text,
                    functionDeclaration.FunctionDeclarator.ParameterList.GetParameters().Select(parameter => parameter.Type.GetToken().Text).ToArray()))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionDeclarationConflictingFunctionName, functionDeclaration.FunctionDeclarator.IdentifierName);
                }

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

                RegisterStatementList(functionDeclaration.Statements, context);

                context.KnownLocalVariables.Clear();
            }
        }

        private static void RegisterVariableDeclarator(JassVariableOrArrayDeclaratorSyntax declarator, bool isGlobalVariable, JassMapScriptAdapterContext context)
        {
            if (!context.KnownTypes.ContainsKey(declarator.GetVariableType().GetToken().Text))
            {
                context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableDeclarationUnknownType, declarator.GetVariableType(), declarator.GetIdentifierName());
            }

            if (declarator is JassVariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Value is not null)
                {
                    RegisterExpression(variableDeclarator.Value.Expression, context);
                }
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
        }

        private static void RegisterExpression(JassExpressionSyntax expression, JassMapScriptAdapterContext context)
        {
            if (expression is JassBinaryExpressionSyntax binaryExpression)
            {
                RegisterExpression(binaryExpression.Left, context);
                RegisterExpression(binaryExpression.Right, context);
            }
            else if (expression is JassUnaryExpressionSyntax unaryExpression)
            {
                RegisterExpression(unaryExpression.Expression, context);
            }
            else if (expression is JassParenthesizedExpressionSyntax parenthesizedExpression)
            {
                RegisterExpression(parenthesizedExpression.Expression, context);
            }
            else if (expression is JassInvocationExpressionSyntax invocationExpression)
            {
                if (context.KnownFunctions.TryGetValue(invocationExpression.IdentifierName.Token.Text, out var knownFunctionParameters))
                {
                    if (knownFunctionParameters.Length != invocationExpression.ArgumentList.ArgumentList.Items.Length)
                    {
                        context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, invocationExpression.IdentifierName, knownFunctionParameters.Length, invocationExpression.ArgumentList.ArgumentList.Items.Length);
                    }
                    else
                    {
                        for (var i = 0; i < knownFunctionParameters.Length; i++)
                        {
                            RegisterExpression(invocationExpression.ArgumentList.ArgumentList.Items[i], context);
                        }
                    }
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationUnknownFunctionIdentifier, invocationExpression.IdentifierName, "invocation expression");
                }
            }
            else if (expression is JassFunctionReferenceExpressionSyntax functionReferenceExpression)
            {
                if (context.KnownFunctions.TryGetValue(functionReferenceExpression.IdentifierName.Token.Text, out var knownFunctionParameters))
                {
                    if (knownFunctionParameters.Length != 0)
                    {
                        context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionReferenceHasParameters, functionReferenceExpression.IdentifierName, knownFunctionParameters.Length);
                    }
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.FunctionReferenceUnknownIdentifier, functionReferenceExpression.IdentifierName);
                }
            }
            else if (expression is JassIdentifierNameSyntax identifierName)
            {
                if (!context.KnownLocalVariables.ContainsKey(identifierName.Token.Text) &&
                    !context.KnownGlobalVariables.ContainsKey(identifierName.Token.Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.VariableReferenceUnknownIdentifier, identifierName);
                }
            }
            else if (expression is JassElementAccessExpressionSyntax elementAccessExpression)
            {
                if (!context.KnownLocalVariables.ContainsKey(elementAccessExpression.IdentifierName.Token.Text) &&
                    !context.KnownGlobalVariables.ContainsKey(elementAccessExpression.IdentifierName.Token.Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.ArrayReferenceUnknownIdentifier, elementAccessExpression.IdentifierName);
                }
            }
        }

        private static void RegisterStatementList(ImmutableArray<JassStatementSyntax> statements, JassMapScriptAdapterContext context)
        {
            foreach (var statement in statements)
            {
                RegisterStatement(statement, context);
            }
        }

        private static void RegisterStatement(JassStatementSyntax statement, JassMapScriptAdapterContext context)
        {
            if (statement is JassSetStatementSyntax setStatement)
            {
                if (!context.KnownLocalVariables.ContainsKey(setStatement.IdentifierName.Token.Text) &&
                    !context.KnownGlobalVariables.ContainsKey(setStatement.IdentifierName.Token.Text))
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.SetStatementUnknownVariableIdentifier, setStatement.IdentifierName);
                }

                if (setStatement.ElementAccessClause is not null)
                {
                    RegisterExpression(setStatement.ElementAccessClause.Expression, context);
                }

                RegisterExpression(setStatement.Value.Expression, context);
            }
            else if (statement is JassCallStatementSyntax callStatement)
            {
                if (context.KnownFunctions.TryGetValue(callStatement.IdentifierName.Token.Text, out var knownFunctionParameters))
                {
                    if (knownFunctionParameters.Length != callStatement.ArgumentList.ArgumentList.Items.Length)
                    {
                        context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationParameterCountMismatch, callStatement.IdentifierName, knownFunctionParameters.Length, callStatement.ArgumentList.ArgumentList.Items.Length);
                    }
                    else
                    {
                        for (var i = 0; i < knownFunctionParameters.Length; i++)
                        {
                            RegisterExpression(callStatement.ArgumentList.ArgumentList.Items[i], context);
                        }
                    }
                }
                else
                {
                    context.AdaptFileContext.ReportDiagnostic(DiagnosticRule.MapScript.InvocationUnknownFunctionIdentifier, callStatement.IdentifierName, "call statement");
                }
            }
            else if (statement is JassDebugStatementSyntax debugStatement)
            {
                RegisterStatement(debugStatement.Statement, context);
            }
            else if (statement is JassExitStatementSyntax exitStatement)
            {
                RegisterExpression(exitStatement.Condition, context);
            }
            else if (statement is JassIfStatementSyntax ifStatement)
            {
                RegisterExpression(ifStatement.IfClause.IfClauseDeclarator.Condition, context);
                RegisterStatementList(ifStatement.IfClause.Statements, context);

                foreach (var elseIfClause in ifStatement.ElseIfClauses)
                {
                    RegisterExpression(elseIfClause.ElseIfClauseDeclarator.Condition, context);
                    RegisterStatementList(elseIfClause.Statements, context);
                }

                if (ifStatement.ElseClause is not null)
                {
                    RegisterStatementList(ifStatement.ElseClause.Statements, context);
                }
            }
            else if (statement is JassLocalVariableDeclarationStatementSyntax localVariableDeclarationStatement)
            {
                RegisterVariableDeclarator(localVariableDeclarationStatement.Declarator, false, context);
            }
            else if (statement is JassLoopStatementSyntax loopStatement)
            {
                RegisterStatementList(loopStatement.Statements, context);
            }
            else if (statement is JassReturnStatementSyntax returnStatement)
            {
                if (returnStatement.Value is not null)
                {
                    RegisterExpression(returnStatement.Value, context);
                }
            }
        }

        private bool TryAdaptDummy<TSyntax>(JassMapScriptAdapterContext context, TSyntax? syntax, out TSyntax? adapted)
            where TSyntax : class
        {
            adapted = null;
            return false;
        }
    }
}