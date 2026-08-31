namespace MyBlazorApp.Core.Expressions;

/// <summary>Malformed input: mismatched parens, unknown token, incomplete expression.</summary>
internal sealed class CalculatorSyntaxException(string message) : Exception(message);

/// <summary>Well-formed input whose evaluation is mathematically undefined (divide by zero, sqrt of a negative, etc).</summary>
internal sealed class CalculatorMathException(string message) : Exception(message);
