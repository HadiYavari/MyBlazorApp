namespace MyBlazorApp.Core.Expressions;

/// <summary>
/// Recursive-descent parser producing an AST that respects standard mathematical
/// precedence and (through recursion into parenthesized sub-expressions) arbitrary
/// nesting of parentheses.
///
/// Grammar (highest to lowest precedence):
///   primary    := NUMBER | CONSTANT | '(' additive ')' | IDENTIFIER '(' additive (',' additive)* ')'
///   postfix    := primary ('!' | '%' | '²' | '³' | '⁻¹')*
///   power      := postfix (('^' | 'ʸ√') unary)?      // right-associative
///   unary      := ('-' | '+') unary | power
///   multiplicative := unary (('×' | '÷') unary)*
///   additive   := multiplicative (('+' | '-') multiplicative)*
/// </summary>
internal sealed class ExpressionParser(IReadOnlyList<Token> tokens)
{
    private int _pos;

    private Token Current => tokens[_pos];

    public Expr ParseExpression()
    {
        var expr = ParseAdditive();
        if (Current.Type != TokenType.End)
        {
            throw new CalculatorSyntaxException("Syntax ERROR");
        }
        return expr;
    }

    private Token Advance()
    {
        var token = Current;
        if (_pos < tokens.Count - 1)
        {
            _pos++;
        }
        return token;
    }

    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
        {
            throw new CalculatorSyntaxException("Syntax ERROR");
        }
        return Advance();
    }

    private Expr ParseAdditive()
    {
        var left = ParseMultiplicative();
        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Advance().Type == TokenType.Plus ? '+' : '-';
            var right = ParseMultiplicative();
            left = new BinaryExpr(left, op, right);
        }
        return left;
    }

    private Expr ParseMultiplicative()
    {
        var left = ParseUnary();
        while (Current.Type is TokenType.Multiply or TokenType.Divide)
        {
            var op = Advance().Type == TokenType.Multiply ? '×' : '÷';
            var right = ParseUnary();
            left = new BinaryExpr(left, op, right);
        }
        return left;
    }

    private Expr ParseUnary()
    {
        if (Current.Type == TokenType.Minus)
        {
            Advance();
            return new UnaryExpr('-', ParseUnary());
        }
        if (Current.Type == TokenType.Plus)
        {
            Advance();
            return ParseUnary();
        }
        return ParsePower();
    }

    private Expr ParsePower()
    {
        var left = ParsePostfix();

        if (Current.Type == TokenType.Power)
        {
            Advance();
            return new BinaryExpr(left, '^', ParseUnary());
        }
        if (Current.Type == TokenType.NRoot)
        {
            Advance();
            return new BinaryExpr(left, 'r', ParseUnary());
        }

        return left;
    }

    private Expr ParsePostfix()
    {
        var expr = ParsePrimary();

        while (true)
        {
            switch (Current.Type)
            {
                case TokenType.Factorial:
                    Advance();
                    expr = new PostfixExpr(expr, "!");
                    continue;
                case TokenType.Percent:
                    Advance();
                    expr = new PostfixExpr(expr, "%");
                    continue;
                case TokenType.Square:
                    Advance();
                    expr = new PostfixExpr(expr, "²");
                    continue;
                case TokenType.Cube:
                    Advance();
                    expr = new PostfixExpr(expr, "³");
                    continue;
                case TokenType.Inverse:
                    Advance();
                    expr = new PostfixExpr(expr, "⁻¹");
                    continue;
                default:
                    return expr;
            }
        }
    }

    private Expr ParsePrimary()
    {
        if (Current.Type == TokenType.Number)
        {
            return new NumberExpr(Advance().NumberValue);
        }

        if (Current.Type == TokenType.LeftParen)
        {
            Advance();
            var inner = ParseAdditive();
            Expect(TokenType.RightParen);
            return inner;
        }

        if (Current.Type == TokenType.Identifier)
        {
            var name = Advance().Text;

            if (name is "pi")
            {
                return new ConstantExpr("π");
            }
            if (name is "e")
            {
                return new ConstantExpr("e");
            }

            Expect(TokenType.LeftParen);
            var args = new List<Expr> { ParseAdditive() };
            while (Current.Type == TokenType.Comma)
            {
                Advance();
                args.Add(ParseAdditive());
            }
            Expect(TokenType.RightParen);

            return new FunctionExpr(name, args);
        }

        throw new CalculatorSyntaxException("Syntax ERROR");
    }
}
