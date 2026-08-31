namespace MyBlazorApp.Core.Expressions;

public readonly struct ExpressionEvaluationResult
{
    private ExpressionEvaluationResult(double? value, CalculatorErrorKind errorKind, string? errorMessage)
    {
        Value = value;
        ErrorKind = errorKind;
        ErrorMessage = errorMessage;
    }

    public double? Value { get; }

    public CalculatorErrorKind ErrorKind { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess => ErrorKind == CalculatorErrorKind.None;

    public static ExpressionEvaluationResult Success(double value) =>
        new(value, CalculatorErrorKind.None, null);

    public static ExpressionEvaluationResult SyntaxError(string message = "Syntax ERROR") =>
        new(null, CalculatorErrorKind.Syntax, message);

    public static ExpressionEvaluationResult MathError(string message = "Math ERROR") =>
        new(null, CalculatorErrorKind.Math, message);
}
