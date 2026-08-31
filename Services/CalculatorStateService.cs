using MyBlazorApp.Core;

namespace MyBlazorApp.Services;

public sealed class CalculatorStateService(ICalculatorEngine engine)
{
    public double FirstNumber { get; set; }

    public double SecondNumber { get; set; }

    public CalculatorOperation SelectedOperation { get; private set; }

    public double? Result { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void SelectOperation(CalculatorOperation operation)
    {
        SelectedOperation = operation;
        Result = null;
        ErrorMessage = null;
    }

    public void Calculate()
    {
        var outcome = engine.Calculate(FirstNumber, SecondNumber, SelectedOperation);
        Result = outcome.IsSuccess ? outcome.Value : null;
        ErrorMessage = outcome.IsSuccess ? null : outcome.ErrorMessage;
    }
}
