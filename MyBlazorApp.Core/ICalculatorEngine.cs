namespace MyBlazorApp.Core;

public interface ICalculatorEngine
{
    CalculationResult Calculate(double first, double second, CalculatorOperation operation);
}
