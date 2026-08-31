namespace MyBlazorApp.Core.Expressions;

internal sealed class ExpressionEvaluator(AngleUnit angleUnit)
{
    public double Evaluate(Expr expr) => expr switch
    {
        NumberExpr n => n.Value,
        ConstantExpr c => EvaluateConstant(c.Name),
        UnaryExpr u => EvaluateUnary(u),
        BinaryExpr b => EvaluateBinary(b),
        PostfixExpr p => EvaluatePostfix(p),
        FunctionExpr f => EvaluateFunction(f),
        _ => throw new CalculatorSyntaxException("Syntax ERROR")
    };

    private static double EvaluateConstant(string name) => name switch
    {
        "π" => Math.PI,
        "e" => Math.E,
        _ => throw new CalculatorSyntaxException("Syntax ERROR")
    };

    private double EvaluateUnary(UnaryExpr u)
    {
        var value = Evaluate(u.Operand);
        return u.Op == '-' ? -value : value;
    }

    private double EvaluateBinary(BinaryExpr b)
    {
        // Context-aware percent: "200 + 10%" means 200 + 200*10%, "200 x 10%" means 200*10%.
        if (b.Right is PostfixExpr { Op: "%" } percent && b.Op is '+' or '-' or '×' or '÷')
        {
            var left = Evaluate(b.Left);
            var rawPercent = Evaluate(percent.Operand);
            return b.Op switch
            {
                '+' => left + left * (rawPercent / 100.0),
                '-' => left - left * (rawPercent / 100.0),
                '×' => left * (rawPercent / 100.0),
                '÷' => DivideChecked(left, rawPercent / 100.0),
                _ => throw new CalculatorSyntaxException("Syntax ERROR")
            };
        }

        var l = Evaluate(b.Left);
        var r = Evaluate(b.Right);

        return b.Op switch
        {
            '+' => l + r,
            '-' => l - r,
            '×' => l * r,
            '÷' => DivideChecked(l, r),
            '^' => PowerChecked(l, r),
            'r' => NRootChecked(l, r),
            _ => throw new CalculatorSyntaxException("Syntax ERROR")
        };
    }

    private static double DivideChecked(double left, double right)
    {
        if (right == 0)
        {
            throw new CalculatorMathException("Cannot divide by zero.");
        }
        return left / right;
    }

    private static double PowerChecked(double baseValue, double exponent)
    {
        var result = Math.Pow(baseValue, exponent);
        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return result;
    }

    private static double NRootChecked(double index, double radicand)
    {
        if (index == 0)
        {
            throw new CalculatorMathException("Math ERROR");
        }

        if (radicand < 0)
        {
            var isOddIntegerIndex = index == Math.Floor(index) && (long)index % 2 != 0;
            if (!isOddIntegerIndex)
            {
                throw new CalculatorMathException("Math ERROR");
            }
            return -Math.Pow(-radicand, 1.0 / index);
        }

        return Math.Pow(radicand, 1.0 / index);
    }

    private double EvaluatePostfix(PostfixExpr p)
    {
        var value = Evaluate(p.Operand);
        return p.Op switch
        {
            "!" => Factorial(value),
            "%" => value / 100.0,
            "²" => PowerChecked(value, 2),
            "³" => PowerChecked(value, 3),
            "⁻¹" => Reciprocal(value),
            _ => throw new CalculatorSyntaxException("Syntax ERROR")
        };
    }

    private static double Reciprocal(double value)
    {
        if (value == 0)
        {
            throw new CalculatorMathException("Cannot divide by zero.");
        }
        return 1.0 / value;
    }

    private static double Factorial(double value)
    {
        if (value < 0 || value != Math.Floor(value) || value > 170)
        {
            throw new CalculatorMathException("Math ERROR");
        }

        double result = 1;
        for (var i = 2; i <= (int)value; i++)
        {
            result *= i;
        }
        return result;
    }

    private double EvaluateFunction(FunctionExpr f)
    {
        if (f.Name == "logy")
        {
            if (f.Args.Count != 2)
            {
                throw new CalculatorSyntaxException("Syntax ERROR");
            }

            var baseValue = Evaluate(f.Args[0]);
            var x = Evaluate(f.Args[1]);
            if (baseValue <= 0 || baseValue == 1 || x <= 0)
            {
                throw new CalculatorMathException("Math ERROR");
            }
            return Math.Log(x, baseValue);
        }

        if (f.Args.Count != 1)
        {
            throw new CalculatorSyntaxException("Syntax ERROR");
        }

        var arg = Evaluate(f.Args[0]);

        return f.Name switch
        {
            "sin" => Math.Sin(ToRadians(arg)),
            "cos" => Math.Cos(ToRadians(arg)),
            "tan" => Tan(arg),
            "asin" => Asin(arg),
            "acos" => Acos(arg),
            "atan" => FromRadians(Math.Atan(arg)),
            "sinh" => Math.Sinh(arg),
            "cosh" => Math.Cosh(arg),
            "tanh" => Math.Tanh(arg),
            "asinh" => Math.Asinh(arg),
            "acosh" => Acosh(arg),
            "atanh" => Atanh(arg),
            "log" => Log10(arg),
            "ln" => Ln(arg),
            "sqrt" => Sqrt(arg),
            "cbrt" => Math.Cbrt(arg),
            "abs" => Math.Abs(arg),
            _ => throw new CalculatorSyntaxException($"Unknown function '{f.Name}'.")
        };
    }

    private double ToRadians(double angle) => angleUnit switch
    {
        AngleUnit.Degrees => angle * Math.PI / 180.0,
        AngleUnit.Gradians => angle * Math.PI / 200.0,
        _ => angle
    };

    private double FromRadians(double radians) => angleUnit switch
    {
        AngleUnit.Degrees => radians * 180.0 / Math.PI,
        AngleUnit.Gradians => radians * 200.0 / Math.PI,
        _ => radians
    };

    private double Tan(double arg)
    {
        var radians = ToRadians(arg);
        if (Math.Abs(Math.Cos(radians)) < 1e-10)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return Math.Tan(radians);
    }

    private double Asin(double arg)
    {
        if (arg is < -1 or > 1)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return FromRadians(Math.Asin(arg));
    }

    private double Acos(double arg)
    {
        if (arg is < -1 or > 1)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return FromRadians(Math.Acos(arg));
    }

    private static double Acosh(double arg)
    {
        if (arg < 1)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return Math.Acosh(arg);
    }

    private static double Atanh(double arg)
    {
        if (arg is <= -1 or >= 1)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return Math.Atanh(arg);
    }

    private static double Log10(double arg)
    {
        if (arg <= 0)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return Math.Log10(arg);
    }

    private static double Ln(double arg)
    {
        if (arg <= 0)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return Math.Log(arg);
    }

    private static double Sqrt(double arg)
    {
        if (arg < 0)
        {
            throw new CalculatorMathException("Math ERROR");
        }
        return Math.Sqrt(arg);
    }
}
