using MyBlazorApp.Core.Expressions;

namespace MyBlazorApp.Core.Tests.Expressions;

public class ExpressionCalculatorEngineTests
{
    private readonly ExpressionCalculatorEngine _engine = new();

    private double AssertSuccess(string expression, AngleUnit angleUnit = AngleUnit.Degrees)
    {
        var result = _engine.Evaluate(expression, angleUnit);
        Assert.True(result.IsSuccess, $"Expected success for '{expression}' but got {result.ErrorKind}: {result.ErrorMessage}");
        return result.Value!.Value;
    }

    private void AssertMathError(string expression, AngleUnit angleUnit = AngleUnit.Degrees)
    {
        var result = _engine.Evaluate(expression, angleUnit);
        Assert.Equal(CalculatorErrorKind.Math, result.ErrorKind);
    }

    private void AssertSyntaxError(string expression)
    {
        var result = _engine.Evaluate(expression, AngleUnit.Degrees);
        Assert.Equal(CalculatorErrorKind.Syntax, result.ErrorKind);
    }

    // ---- Basic arithmetic ----

    [Theory]
    [InlineData("2+3", 5)]
    [InlineData("5-3", 2)]
    [InlineData("4×5", 20)]
    [InlineData("4*5", 20)]
    [InlineData("10÷4", 2.5)]
    [InlineData("10/4", 2.5)]
    [InlineData("-5+3", -2)]
    [InlineData("2--3", 5)]
    public void BasicArithmetic(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 10);
    }

    [Fact]
    public void DivideByZero_IsMathError()
    {
        AssertMathError("10÷0");
    }

    // ---- Precedence & parentheses ----

    [Theory]
    [InlineData("2+3×4", 14)]
    [InlineData("(2+3)×4", 20)]
    [InlineData("2×(3+4)×2", 28)]
    [InlineData("((1+2)×(3+4))", 21)]
    [InlineData("2^3^2", 512)]
    [InlineData("-2^2", -4)]
    public void OperatorPrecedenceAndParentheses(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 10);
    }

    [Theory]
    [InlineData("(2+3")]
    [InlineData("2+3)")]
    [InlineData("2++")]
    [InlineData("")]
    [InlineData("()")]
    public void MalformedExpressions_AreSyntaxErrors(string expression)
    {
        AssertSyntaxError(expression);
    }

    // ---- Powers & roots ----

    [Theory]
    [InlineData("5²", 25)]
    [InlineData("3³", 27)]
    [InlineData("2^10", 1024)]
    [InlineData("sqrt(9)", 3)]
    [InlineData("√(16)", 4)]
    [InlineData("cbrt(27)", 3)]
    [InlineData("∛(-8)", -2)]
    public void PowersAndRoots(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 10);
    }

    [Fact]
    public void NthRoot_UsesLeftAsIndexAndRightAsRadicand()
    {
        Assert.Equal(2, AssertSuccess("3ʸ√8"), precision: 10);
    }

    [Fact]
    public void SquareRoot_OfNegative_IsMathError()
    {
        AssertMathError("sqrt(-1)");
    }

    [Fact]
    public void Reciprocal_OfZero_IsMathError()
    {
        AssertMathError("0⁻¹");
    }

    [Fact]
    public void Reciprocal_ComputesOneOverX()
    {
        Assert.Equal(0.25, AssertSuccess("4⁻¹"), precision: 10);
    }

    // ---- Logarithms ----

    [Theory]
    [InlineData("log(100)", 2)]
    [InlineData("ln(1)", 0)]
    [InlineData("logy(2,8)", 3)]
    public void Logarithms(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 10);
    }

    [Fact]
    public void Log_OfZero_IsMathError() => AssertMathError("log(0)");

    [Fact]
    public void Log_OfNegative_IsMathError() => AssertMathError("log(-5)");

    [Fact]
    public void Ln_OfZero_IsMathError() => AssertMathError("ln(0)");

    // ---- Trigonometry ----

    [Theory]
    [InlineData("sin(30)", 0.5)]
    [InlineData("cos(60)", 0.5)]
    [InlineData("sin(90)", 1)]
    public void Trig_Degrees(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression, AngleUnit.Degrees), precision: 6);
    }

    [Fact]
    public void Trig_Radians()
    {
        Assert.Equal(1, AssertSuccess("sin(1.5707963267948966)", AngleUnit.Radians), precision: 6);
    }

    [Fact]
    public void Trig_Gradians()
    {
        Assert.Equal(1, AssertSuccess("sin(100)", AngleUnit.Gradians), precision: 6);
    }

    [Fact]
    public void Tan_At90Degrees_IsMathError()
    {
        AssertMathError("tan(90)");
    }

    [Theory]
    [InlineData("asin(0.5)", 30)]
    [InlineData("acos(0.5)", 60)]
    [InlineData("atan(1)", 45)]
    public void InverseTrig_Degrees(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression, AngleUnit.Degrees), precision: 6);
    }

    [Fact]
    public void Asin_OutOfDomain_IsMathError() => AssertMathError("asin(2)");

    [Fact]
    public void Acos_OutOfDomain_IsMathError() => AssertMathError("acos(-2)");

    [Theory]
    [InlineData("sinh(0)", 0)]
    [InlineData("cosh(0)", 1)]
    [InlineData("tanh(0)", 0)]
    public void HyperbolicTrig(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 6);
    }

    [Fact]
    public void Acosh_BelowOne_IsMathError() => AssertMathError("acosh(0)");

    [Fact]
    public void Atanh_OutOfDomain_IsMathError() => AssertMathError("atanh(1)");

    // ---- Other functions ----

    [Theory]
    [InlineData("5!", 120)]
    [InlineData("0!", 1)]
    [InlineData("1!", 1)]
    public void Factorial(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 10);
    }

    [Fact]
    public void Factorial_OfNegative_IsMathError() => AssertMathError("(-1)!");

    [Fact]
    public void Factorial_OfNonInteger_IsMathError() => AssertMathError("2.5!");

    [Theory]
    [InlineData("abs(-5)", 5)]
    [InlineData("abs(5)", 5)]
    public void AbsoluteValue(string expression, double expected)
    {
        Assert.Equal(expected, AssertSuccess(expression), precision: 10);
    }

    // ---- Percent (context-aware) ----

    [Fact]
    public void Percent_AfterAddition_IsPercentOfLeftOperand()
    {
        Assert.Equal(220, AssertSuccess("200+10%"), precision: 10);
    }

    [Fact]
    public void Percent_AfterSubtraction_IsPercentOfLeftOperand()
    {
        Assert.Equal(180, AssertSuccess("200-10%"), precision: 10);
    }

    [Fact]
    public void Percent_AfterMultiplication_IsDirectFraction()
    {
        Assert.Equal(20, AssertSuccess("200×10%"), precision: 10);
    }

    [Fact]
    public void Percent_Standalone_IsValueDividedByOneHundred()
    {
        Assert.Equal(0.5, AssertSuccess("50%"), precision: 10);
    }

    // ---- Constants ----

    [Fact]
    public void Pi_MatchesMathPI()
    {
        Assert.Equal(Math.PI, AssertSuccess("pi"), precision: 10);
    }

    [Fact]
    public void EulersNumber_MatchesMathE()
    {
        Assert.Equal(Math.E, AssertSuccess("e"), precision: 10);
    }

    [Fact]
    public void TenToThePower_ViaCaretAndConstant()
    {
        Assert.Equal(1000, AssertSuccess("10^3"), precision: 10);
    }

    [Fact]
    public void EulersNumber_ToThePower_ViaCaretAndConstant()
    {
        Assert.Equal(Math.E, AssertSuccess("e^1"), precision: 10);
    }
}
