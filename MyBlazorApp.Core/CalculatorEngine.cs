namespace MyBlazorApp.Core;

public sealed class CalculatorEngine : ICalculatorEngine
{
    public CalculationResult Calculate(double first, double second, CalculatorOperation operation) =>
        operation switch
        {
            CalculatorOperation.Add => CalculationResult.Success(first + second),
            CalculatorOperation.Subtract => CalculationResult.Success(first - second),
            CalculatorOperation.Multiply => CalculationResult.Success(first * second),
            CalculatorOperation.Divide => Divide(first, second),
            _ => CalculationResult.Failure("Please choose an operation.")
        };

    private static CalculationResult Divide(double first, double second) =>
        second == 0
            ? CalculationResult.Failure("Cannot divide by zero.")
            : CalculationResult.Success(first / second);
}
