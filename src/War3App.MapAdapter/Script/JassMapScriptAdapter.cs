using System;
using System.IO;
using System.Linq;
using System.Text;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.CodeAnalysis.Jass;
using War3Net.CodeAnalysis.Jass.Syntax;

namespace War3App.MapAdapter.Script
{
    public sealed partial class JassMapScriptAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Map Script (JASS)";

        public bool IsTextFile => true;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var commonJPath = Path.Combine(targetPatch.GameDataPath, PathConstants.CommonJPath);
                if (!File.Exists(commonJPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = commonJPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var blizzardJPath = Path.Combine(targetPatch.GameDataPath, PathConstants.BlizzardJPath);
                if (!File.Exists(blizzardJPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = blizzardJPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var commonJText = File.ReadAllText(commonJPath);
                var commonJCompilationUnit = JassSyntaxFactory.ParseCompilationUnit(commonJText);

                var blizzardJText = File.ReadAllText(blizzardJPath);
                var blizzardJCompilationUnit = JassSyntaxFactory.ParseCompilationUnit(blizzardJText);

                string scriptText;
                using (var reader = new StreamReader(stream, leaveOpen: true))
                {
                    scriptText = reader.ReadToEnd();
                }

                var compilationUnit = JassSyntaxFactory.ParseCompilationUnit(scriptText);

                try
                {
                    var context = new JassMapScriptAdapterContext();

                    foreach (var declaration in commonJCompilationUnit.Declarations)
                    {
                        RegisterDeclaration(declaration, context);
                    }

                    foreach (var declaration in blizzardJCompilationUnit.Declarations)
                    {
                        RegisterDeclaration(declaration, context);
                    }

                    // Common.j and Blizzard.j should not cause any diagnostics.
                    if (context.Diagnostics.Count > 0)
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.AdapterError,
                            Diagnostics = context.Diagnostics.ToArray(),
                        };
                    }

                    if (TryAdaptCompilationUnit(context, compilationUnit, out var adaptedCompilationUnit))
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new StreamWriter(memoryStream, new UTF8Encoding(false, true), leaveOpen: true);

                        var renderer = new JassRenderer(writer);
                        renderer.Render(adaptedCompilationUnit);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            Diagnostics = context.Diagnostics.ToArray(),
                            AdaptedFileStream = memoryStream,
                        };
                    }

                    if (context.Diagnostics.Count == 0)
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Compatible,
                        };
                    }
                    else
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Incompatible,
                            Diagnostics = context.Diagnostics.ToArray(),
                        };
                    }
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }

        private static void RegisterDeclaration(IDeclarationSyntax declaration, JassMapScriptAdapterContext context)
        {
            if (declaration is JassTypeDeclarationSyntax typeDeclaration)
            {
                if (!context.KnownTypes.ContainsKey(typeDeclaration.BaseType.TypeName.Name))
                {
                    context.Diagnostics.Add($"Unknown base type '{typeDeclaration.BaseType}'.");
                }

                context.KnownTypes.Add(typeDeclaration.IdentifierName.Name, typeDeclaration.BaseType.TypeName.Name);
            }
            else if (declaration is JassGlobalDeclarationListSyntax globalDeclarationList)
            {
                foreach (var global in globalDeclarationList.Globals)
                {
                    if (global is JassGlobalDeclarationSyntax globalDeclaration)
                    {
                        RegisterVariableDeclarator(globalDeclaration.Declarator, true, context);
                    }
                }
            }
            else if (declaration is JassNativeFunctionDeclarationSyntax nativeFunctionDeclaration)
            {
                if (!context.KnownFunctions.TryAdd(
                    nativeFunctionDeclaration.FunctionDeclarator.IdentifierName.Name,
                    nativeFunctionDeclaration.FunctionDeclarator.ParameterList.Parameters.Select(parameter => parameter.Type.TypeName.Name).ToArray()))
                {
                    context.Diagnostics.Add($"Duplicate function '{nativeFunctionDeclaration.FunctionDeclarator.IdentifierName}'.");
                }
            }
            else if (declaration is JassFunctionDeclarationSyntax functionDeclaration)
            {
                if (!context.KnownFunctions.TryAdd(
                    functionDeclaration.FunctionDeclarator.IdentifierName.Name,
                    functionDeclaration.FunctionDeclarator.ParameterList.Parameters.Select(parameter => parameter.Type.TypeName.Name).ToArray()))
                {
                    context.Diagnostics.Add($"Duplicate function '{functionDeclaration.FunctionDeclarator.IdentifierName}'.");
                }

                foreach (var parameter in functionDeclaration.FunctionDeclarator.ParameterList.Parameters)
                {
                    if (!context.KnownTypes.ContainsKey(parameter.Type.TypeName.Name))
                    {
                        context.Diagnostics.Add($"Unknown variable type '{parameter.Type}'.");
                    }

                    if (!context.KnownLocalVariables.TryAdd(parameter.IdentifierName.Name, parameter.Type.TypeName.Name))
                    {
                        context.Diagnostics.Add($"Duplicate local variable '{parameter.IdentifierName}'.");
                    }
                }

                RegisterStatementList(functionDeclaration.Body, context);

                context.KnownLocalVariables.Clear();
            }
        }

        private static void RegisterVariableDeclarator(IVariableDeclaratorSyntax declarator, bool isGlobalVariable, JassMapScriptAdapterContext context)
        {
            if (!context.KnownTypes.ContainsKey(declarator.Type.TypeName.Name))
            {
                context.Diagnostics.Add($"Unknown variable type '{declarator.Type}'.");
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
                if (!context.KnownGlobalVariables.TryAdd(declarator.IdentifierName.Name, declarator.Type.TypeName.Name))
                {
                    context.Diagnostics.Add($"Duplicate global variable '{declarator.IdentifierName}'.");
                }
            }
            else
            {
                if (!context.KnownLocalVariables.TryAdd(declarator.IdentifierName.Name, declarator.Type.TypeName.Name))
                {
                    context.Diagnostics.Add($"Duplicate local variable '{declarator.IdentifierName}'.");
                }
            }
        }

        private static void RegisterExpression(IExpressionSyntax expression, JassMapScriptAdapterContext context)
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
                if (context.KnownFunctions.TryGetValue(invocationExpression.IdentifierName.Name, out var knownFunctionParameters))
                {
                    if (knownFunctionParameters.Length != invocationExpression.Arguments.Arguments.Length)
                    {
                        context.Diagnostics.Add($"Invalid function invocation: '{invocationExpression.IdentifierName}' expected {knownFunctionParameters.Length} parameters but got {invocationExpression.Arguments.Arguments.Length}.");
                    }
                    else
                    {
                        for (var i = 0; i < knownFunctionParameters.Length; i++)
                        {
                            RegisterExpression(invocationExpression.Arguments.Arguments[i], context);
                        }
                    }
                }
                else
                {
                    context.Diagnostics.Add($"Unknown function '{invocationExpression.IdentifierName}'.");
                }
            }
            else if (expression is JassFunctionReferenceExpressionSyntax functionReferenceExpression)
            {
                if (context.KnownFunctions.TryGetValue(functionReferenceExpression.IdentifierName.Name, out var knownFunctionParameters))
                {
                    if (knownFunctionParameters.Length != 0)
                    {
                        context.Diagnostics.Add($"Invalid function reference: '{functionReferenceExpression.IdentifierName}' should not have any parameters.");
                    }
                }
                else
                {
                    context.Diagnostics.Add($"Unknown function '{functionReferenceExpression.IdentifierName}'.");
                }
            }
            else if (expression is JassVariableReferenceExpressionSyntax variableReferenceExpression)
            {
                if (!context.KnownLocalVariables.ContainsKey(variableReferenceExpression.IdentifierName.Name) &&
                    !context.KnownGlobalVariables.ContainsKey(variableReferenceExpression.IdentifierName.Name))
                {
                    context.Diagnostics.Add($"Unknown variable '{variableReferenceExpression.IdentifierName}'.");
                }
            }
            else if (expression is JassArrayReferenceExpressionSyntax arrayReferenceExpression)
            {
                if (!context.KnownLocalVariables.ContainsKey(arrayReferenceExpression.IdentifierName.Name) &&
                    !context.KnownGlobalVariables.ContainsKey(arrayReferenceExpression.IdentifierName.Name))
                {
                    context.Diagnostics.Add($"Unknown array '{arrayReferenceExpression.IdentifierName}'.");
                }
            }
        }

        private static void RegisterStatementList(JassStatementListSyntax statementList, JassMapScriptAdapterContext context)
        {
            foreach (var statement in statementList.Statements)
            {
                RegisterStatement(statement, context);
            }
        }

        private static void RegisterStatement(IStatementSyntax statement, JassMapScriptAdapterContext context)
        {
            if (statement is JassSetStatementSyntax setStatement)
            {
                if (!context.KnownLocalVariables.ContainsKey(setStatement.IdentifierName.Name) &&
                    !context.KnownGlobalVariables.ContainsKey(setStatement.IdentifierName.Name))
                {
                    context.Diagnostics.Add($"Unknown variable '{setStatement.IdentifierName}'.");
                }

                if (setStatement.Indexer is not null)
                {
                    RegisterExpression(setStatement.Indexer, context);
                }

                RegisterExpression(setStatement.Value.Expression, context);
            }
            else if (statement is JassCallStatementSyntax callStatement)
            {
                if (context.KnownFunctions.TryGetValue(callStatement.IdentifierName.Name, out var knownFunctionParameters))
                {
                    if (knownFunctionParameters.Length != callStatement.Arguments.Arguments.Length)
                    {
                        context.Diagnostics.Add($"Invalid function invocation: '{callStatement.IdentifierName}' expected {knownFunctionParameters.Length} parameters but got {callStatement.Arguments.Arguments.Length}.");
                    }
                    else
                    {
                        for (var i = 0; i < knownFunctionParameters.Length; i++)
                        {
                            RegisterExpression(callStatement.Arguments.Arguments[i], context);
                        }
                    }
                }
                else
                {
                    context.Diagnostics.Add($"Unknown function '{callStatement.IdentifierName}'.");
                }
            }
            else if (statement is JassDebugStatementSyntax debugStatement)
            {
                RegisterStatement(debugStatement, context);
            }
            else if (statement is JassExitStatementSyntax exitStatement)
            {
                RegisterExpression(exitStatement.Condition, context);
            }
            else if (statement is JassIfStatementSyntax ifStatement)
            {
                RegisterExpression(ifStatement.Condition, context);
                RegisterStatementList(ifStatement.Body, context);

                foreach (var elseIfClause in ifStatement.ElseIfClauses)
                {
                    RegisterExpression(elseIfClause.Condition, context);
                    RegisterStatementList(elseIfClause.Body, context);
                }

                if (ifStatement.ElseClause is not null)
                {
                    RegisterStatementList(ifStatement.ElseClause.Body, context);
                }
            }
            else if (statement is JassLocalVariableDeclarationStatementSyntax localVariableDeclarationStatement)
            {
                RegisterVariableDeclarator(localVariableDeclarationStatement.Declarator, false, context);
            }
            else if (statement is JassLoopStatementSyntax loopStatement)
            {
                RegisterStatementList(loopStatement.Body, context);
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