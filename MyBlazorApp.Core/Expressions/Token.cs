namespace MyBlazorApp.Core.Expressions;

internal enum TokenType
{
    Number,
    Identifier,
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    NRoot,
    LeftParen,
    RightParen,
    Comma,
    Factorial,
    Percent,
    Square,
    Cube,
    Inverse,
    End
}

internal readonly record struct Token(TokenType Type, string Text, double NumberValue = 0);
