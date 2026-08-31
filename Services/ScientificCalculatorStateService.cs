using System.Globalization;
using System.Text.RegularExpressions;
using MyBlazorApp.Core.Expressions;

namespace MyBlazorApp.Services;

/// <summary>
/// Holds the UI state for the scientific calculator: the expression being typed,
/// angle unit, single memory register, and the outcome of the last "=" press.
/// All actual math is delegated to <see cref="IExpressionCalculatorEngine"/>.
/// </summary>
public sealed partial class ScientificCalculatorStateService(IExpressionCalculatorEngine engine)
{
    private static readonly Regex TrailingNumber = TrailingNumberRegex();

    private bool _justCalculated;

    public string Expression { get; set; } = string.Empty;

    public AngleUnit AngleUnit { get; private set; } = AngleUnit.Degrees;

    public double Memory { get; private set; }

    public double? Result { get; private set; }

    public CalculatorErrorKind ErrorKind { get; private set; } = CalculatorErrorKind.None;

    public string? ErrorMessage { get; private set; }

    public bool HasError => ErrorKind != CalculatorErrorKind.None;

    public void AppendDigitOrDecimal(string text)
    {
        BeginEdit(continuesFromResult: false);
        Expression += text;
    }

    public void AppendOperator(string op)
    {
        BeginEdit(continuesFromResult: true);
        Expression += op;
    }

    public void AppendFunction(string functionName)
    {
        BeginEdit(continuesFromResult: false);
        Expression += functionName + "(";
    }

    /// <summary>Inserts raw text (e.g. "10^(" for the 10^x button) as a fresh function-like entry.</summary>
    public void AppendFunctionText(string text)
    {
        BeginEdit(continuesFromResult: false);
        Expression += text;
    }

    public void AppendConstant(string symbol)
    {
        BeginEdit(continuesFromResult: false);
        Expression += symbol;
    }

    public void AppendPostfix(string op)
    {
        BeginEdit(continuesFromResult: true);
        Expression += op;
    }

    public void AppendParen(string paren)
    {
        BeginEdit(continuesFromResult: paren == ")");
        Expression += paren;
    }

    public void AppendComma()
    {
        BeginEdit(continuesFromResult: true);
        Expression += ",";
    }

    public void InsertRandomNumber()
    {
        BeginEdit(continuesFromResult: false);
        var value = Random.Shared.NextDouble();
        Expression += FormatNumber(value);
    }

    public void Backspace()
    {
        ClearErrorIfAny();
        if (Expression.Length == 0)
        {
            return;
        }

        var newLength = Expression.Length - 1;
        if (newLength > 0 && Expression[newLength - 1] == '⁻' && Expression[newLength] == '¹')
        {
            newLength--;
        }
        Expression = Expression[..newLength];
    }

    public void ClearEntry()
    {
        Expression = string.Empty;
        ClearErrorIfAny();
    }

    public void AllClear()
    {
        Expression = string.Empty;
        Result = null;
        ErrorKind = CalculatorErrorKind.None;
        ErrorMessage = null;
        _justCalculated = false;
    }

    public void ToggleSign()
    {
        ClearErrorIfAny();

        var match = TrailingNumber.Match(Expression);
        if (!match.Success)
        {
            Expression += "-";
            return;
        }

        var start = match.Index;
        if (start > 0 && Expression[start - 1] == '-' && IsUnaryPosition(start - 1))
        {
            Expression = Expression[..(start - 1)] + Expression[start..];
        }
        else
        {
            Expression = Expression[..start] + "-" + Expression[start..];
        }
    }

    public void SetAngleUnit(AngleUnit unit) => AngleUnit = unit;

    public void Calculate()
    {
        if (Expression.Length == 0)
        {
            return;
        }

        var outcome = engine.Evaluate(Expression, AngleUnit);
        if (outcome.IsSuccess)
        {
            Result = outcome.Value;
            ErrorKind = CalculatorErrorKind.None;
            ErrorMessage = null;
            Expression = FormatNumber(outcome.Value!.Value);
            _justCalculated = true;
        }
        else
        {
            Result = null;
            ErrorKind = outcome.ErrorKind;
            ErrorMessage = outcome.ErrorMessage;
            _justCalculated = false;
        }
    }

    public void MemoryAdd() => ApplyToMemory(static (memory, value) => memory + value);

    public void MemorySubtract() => ApplyToMemory(static (memory, value) => memory - value);

    public void MemoryStore() => ApplyToMemory(static (_, value) => value);

    public void MemoryRecall()
    {
        BeginEdit(continuesFromResult: false);
        Expression += FormatNumber(Memory);
    }

    public void MemoryClear() => Memory = 0;

    private void ApplyToMemory(Func<double, double, double> combine)
    {
        if (!TryGetCurrentValue(out var value))
        {
            return;
        }

        Memory = combine(Memory, value);
        Result = value;
        ErrorKind = CalculatorErrorKind.None;
        ErrorMessage = null;
        Expression = FormatNumber(value);
        _justCalculated = true;
    }

    private bool TryGetCurrentValue(out double value)
    {
        if (Expression.Length > 0)
        {
            var outcome = engine.Evaluate(Expression, AngleUnit);
            if (outcome.IsSuccess)
            {
                value = outcome.Value!.Value;
                return true;
            }

            Result = null;
            ErrorKind = outcome.ErrorKind;
            ErrorMessage = outcome.ErrorMessage;
            value = 0;
            return false;
        }

        if (Result is { } lastResult)
        {
            value = lastResult;
            return true;
        }

        value = 0;
        return false;
    }

    private void BeginEdit(bool continuesFromResult)
    {
        if (_justCalculated)
        {
            if (!continuesFromResult)
            {
                Expression = string.Empty;
            }
            _justCalculated = false;
        }
        ClearErrorIfAny();
    }

    private void ClearErrorIfAny()
    {
        if (HasError)
        {
            ErrorKind = CalculatorErrorKind.None;
            ErrorMessage = null;
        }
    }

    /// <summary>A '-' is acting as unary negation (not binary subtraction) when it starts
    /// the expression or immediately follows an operator, comma, or open paren.</summary>
    private bool IsUnaryPosition(int minusIndex)
    {
        if (minusIndex == 0)
        {
            return true;
        }

        var previous = Expression[minusIndex - 1];
        return previous is '+' or '-' or '×' or '÷' or '^' or '(' or ',';
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.##########", CultureInfo.InvariantCulture);

    [GeneratedRegex(@"(\d+\.?\d*|\.\d+)$")]
    private static partial Regex TrailingNumberRegex();
}
