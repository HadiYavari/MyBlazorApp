namespace MyBlazorApp.Core.Expressions;

internal abstract record Expr;

internal sealed record NumberExpr(double Value) : Expr;

internal sealed record ConstantExpr(string Name) : Expr;

internal sealed record UnaryExpr(char Op, Expr Operand) : Expr;

/// <summary>Op is one of + - × ÷ ^ (power) or 'r' (n-th root: Left is the index n, Right is the radicand x).</summary>
internal sealed record BinaryExpr(Expr Left, char Op, Expr Right) : Expr;

/// <summary>Op is one of ! % ² ³ ⁻¹, applied to the value that precedes it.</summary>
internal sealed record PostfixExpr(Expr Operand, string Op) : Expr;

internal sealed record FunctionExpr(string Name, IReadOnlyList<Expr> Args) : Expr;
