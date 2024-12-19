namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class MapScript
        {
            public static readonly DiagnosticDescriptor UnsupportedIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Found incompatible identifier: '{0}' ({1}x).",
            };

            public static readonly DiagnosticDescriptor UnsupportedAudioFormat = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Found incompatible audio format: '{0}' ({1}x).",
            };

            public static readonly DiagnosticDescriptor UnsupportedFrameName = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Found incompatible frame name: '{0}' ({1}x).",
            };

            public static readonly DiagnosticDescriptor FunctionReferenceHasParameters = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Functions used in a function reference expression are not allowed to have any parameters, but function '{0}' takes {1} parameters.",
            };

            public static readonly DiagnosticDescriptor FunctionReferenceUnknownIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown function identifier '{0}' in function reference expression.",
            };

            public static readonly DiagnosticDescriptor ArrayReferenceUnknownIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown array identifier '{0}' in array reference expression.",
            };

            public static readonly DiagnosticDescriptor VariableReferenceUnknownIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown variable identifier '{0}' in variable reference expression.",
            };

            public static readonly DiagnosticDescriptor FunctionDeclarationUnknownParameterVariableType = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown variable type '{0}' in function declaration '{1}' for parameter '{2}'.",
            };

            public static readonly DiagnosticDescriptor FunctionDeclarationConflictingParameterVariableName = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Parameter '{0}' in function declaration '{1}' is invalid, because a local variable or parameter with the same identifier name has already been declared.",
            };

            public static readonly DiagnosticDescriptor FunctionDeclarationConflictingFunctionName = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Function declaration '{0}' is invalid, because a function with the same identifier name has already been declared.",
            };

            public static readonly DiagnosticDescriptor VariableDeclarationUnknownType = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown variable type '{0}' in variable declaration '{1}'.",
            };

            public static readonly DiagnosticDescriptor VariableDeclarationConflictingVariableName = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Variable declaration '{0}' is invalid, because a {1} variable with the same identifier name has already been declared.",
            };

            public static readonly DiagnosticDescriptor InvocationParameterCountMismatch = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Function invocation '{0}' is invalid, because this function expects {1} parameters, but got {2}.",
            };

            public static readonly DiagnosticDescriptor InvocationConflictingDialogueTitle = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Duplicate dialogue title '{0}' for sound '{1}'.",
            };

            public static readonly DiagnosticDescriptor InvocationConflictingDialogueText = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Duplicate dialogue text '{0}' for sound '{1}'.",
            };

            public static readonly DiagnosticDescriptor InvocationUnknownFunctionIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown function identifier '{0}' in {1}.",
            };

            public static readonly DiagnosticDescriptor SetStatementUnknownVariableIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown variable identifier '{0}' in set statement.",
            };

            public static readonly DiagnosticDescriptor TypeDeclarationUnknownBaseType = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Unknown base type '{0}' in type declaration '{1}'.",
            };

            public static readonly DiagnosticDescriptor InvocationExpressionShouldBeRemoved = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Invocation of function '{0}' should be removed.",
            };

            public static readonly DiagnosticDescriptor InvocationBlzCreateWithSkinIdRemoved = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "Function invocation '{0}' changed to '{1}' by removing the skinId parameter.",
            };

            public static readonly DiagnosticDescriptor InvocationBlzCreateWithSkinIdDeleted = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Function invocation '{0}' changed to '{1}' by deleting the skinId parameter {2}, which is different from the typeId {3}.",
            };
        }

        public static class MapTriggers
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The file format version {0} is not supported before {1}.",
            };

            public static readonly DiagnosticDescriptor UnsupportedTriggerFunction = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The trigger {0} '{1}' is not supported.",
            };

            public static readonly DiagnosticDescriptor UnsupportedVariableType = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Variable '{0}' is of unsupported type '{1}'.",
            };

            public static readonly DiagnosticDescriptor VariableTypeChanged = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "Changed type of variable '{0}' from '{1}' to '{2}'.",
            };
        }
    }
}