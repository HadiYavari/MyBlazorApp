namespace MyBlazorApp.Core;

public readonly struct CalculationResult
{
    private CalculationResult(double? value, string? errorMessage)
    {
        Value = value;
        ErrorMessage = errorMessage;
    }

    public double? Value { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess => ErrorMessage is null;

    public static CalculationResult Success(double value) => new(value, null);

    public static CalculationResult Failure(string errorMessage) => new(null, errorMessage);
}
