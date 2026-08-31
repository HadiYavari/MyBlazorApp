namespace MyBlazorApp.Core.Expressions;

/// <summary>
/// Evaluates a full algebraic expression (e.g. "2+3×(4-1)") with correct operator
/// precedence and nested parentheses, matching a Casio fx-991-class scientific
/// calculator's function set. See CHANGES.md for the exact button/syntax mapping.
/// </summary>
public sealed class ExpressionCalculatorEngine : IExpressionCalculatorEngine
{
    public ExpressionEvaluationResult Evaluate(string expression, AngleUnit angleUnit)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return ExpressionEvaluationResult.SyntaxError();
        }

        try
        {
            var tokens = Tokenizer.Tokenize(expression);
            var ast = new ExpressionParser(tokens).ParseExpression();
            var value = new ExpressionEvaluator(angleUnit).Evaluate(ast);

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return ExpressionEvaluationResult.MathError();
            }

            return ExpressionEvaluationResult.Success(value);
        }
        catch (CalculatorSyntaxException ex)
        {
            return ExpressionEvaluationResult.SyntaxError(ex.Message);
        }
        catch (CalculatorMathException ex)
        {
            return ExpressionEvaluationResult.MathError(ex.Message);
        }
    }
}
