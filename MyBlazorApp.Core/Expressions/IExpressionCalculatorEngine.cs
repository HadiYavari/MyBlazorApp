namespace MyBlazorApp.Core.Expressions;

public interface IExpressionCalculatorEngine
{
    ExpressionEvaluationResult Evaluate(string expression, AngleUnit angleUnit);
}
