using MyBlazorApp.Core;

namespace MyBlazorApp.Core.Tests;

public class CalculatorEngineTests
{
    private readonly CalculatorEngine _engine = new();

    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(-2, 3, 1)]
    [InlineData(0, 0, 0)]
    public void Add_ReturnsSum(double first, double second, double expected)
    {
        var result = _engine.Calculate(first, second, CalculatorOperation.Add);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(5, 3, 2)]
    [InlineData(3, 5, -2)]
    public void Subtract_ReturnsDifference(double first, double second, double expected)
    {
        var result = _engine.Calculate(first, second, CalculatorOperation.Subtract);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(4, 5, 20)]
    [InlineData(-3, 5, -15)]
    [InlineData(0, 100, 0)]
    public void Multiply_ReturnsProduct(double first, double second, double expected)
    {
        var result = _engine.Calculate(first, second, CalculatorOperation.Multiply);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(10, 4, 2.5)]
    [InlineData(-9, 3, -3)]
    public void Divide_ReturnsQuotient(double first, double second, double expected)
    {
        var result = _engine.Calculate(first, second, CalculatorOperation.Divide);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Divide_ByZero_ReturnsFailure()
    {
        var result = _engine.Calculate(10, 0, CalculatorOperation.Divide);

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot divide by zero.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Calculate_NoOperationSelected_ReturnsFailure()
    {
        var result = _engine.Calculate(10, 5, CalculatorOperation.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Please choose an operation.", result.ErrorMessage);
    }

    [Fact]
    public void Calculate_ChainedOperations_UsesPreviousResultAsInput()
    {
        var first = _engine.Calculate(2, 3, CalculatorOperation.Add);
        Assert.True(first.IsSuccess);

        var second = _engine.Calculate(first.Value!.Value, 4, CalculatorOperation.Multiply);

        Assert.True(second.IsSuccess);
        Assert.Equal(20, second.Value);
    }

    [Fact]
    public void Add_WithDecimals_PreservesPrecisionWithinTolerance()
    {
        var result = _engine.Calculate(0.1, 0.2, CalculatorOperation.Add);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.3, result.Value!.Value, precision: 10);
    }
}
