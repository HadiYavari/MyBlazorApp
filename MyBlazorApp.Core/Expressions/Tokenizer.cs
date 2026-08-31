using System.Globalization;

namespace MyBlazorApp.Core.Expressions;

internal static class Tokenizer
{
    private static readonly HashSet<string> KnownIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "sin", "cos", "tan", "asin", "acos", "atan",
        "sinh", "cosh", "tanh", "asinh", "acosh", "atanh",
        "log", "ln", "sqrt", "cbrt", "abs", "logy",
        "pi", "e"
    };

    public static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < input.Length)
        {
            var ch = input[i];

            if (char.IsWhiteSpace(ch))
            {
                i++;
                continue;
            }

            if (char.IsDigit(ch) || ch == '.')
            {
                var start = i;
                var sawDot = false;
                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                {
                    if (input[i] == '.')
                    {
                        if (sawDot)
                        {
                            throw new CalculatorSyntaxException("Syntax ERROR");
                        }
                        sawDot = true;
                    }
                    i++;
                }

                var text = input[start..i];
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    throw new CalculatorSyntaxException("Syntax ERROR");
                }

                tokens.Add(new Token(TokenType.Number, text, value));
                continue;
            }

            // "ʸ√" must be checked before the general identifier branch below, since 'ʸ'
            // (U+02B8 MODIFIER LETTER SMALL Y) is itself classified as a Unicode letter.
            if (ch == 'ʸ' && i + 1 < input.Length && input[i + 1] == '√')
            {
                tokens.Add(new Token(TokenType.NRoot, "ʸ√"));
                i += 2;
                continue;
            }

            if (char.IsLetter(ch))
            {
                var start = i;
                while (i < input.Length && char.IsLetter(input[i]))
                {
                    i++;
                }

                var text = input[start..i];
                if (!KnownIdentifiers.Contains(text))
                {
                    throw new CalculatorSyntaxException($"Unknown function '{text}'.");
                }

                tokens.Add(new Token(TokenType.Identifier, text.ToLowerInvariant()));
                continue;
            }

            if (ch == '⁻' && i + 1 < input.Length && input[i + 1] == '¹')
            {
                tokens.Add(new Token(TokenType.Inverse, "⁻¹"));
                i += 2;
                continue;
            }

            switch (ch)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+"));
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-"));
                    break;
                case '×':
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "×"));
                    break;
                case '÷':
                case '/':
                    tokens.Add(new Token(TokenType.Divide, "÷"));
                    break;
                case '^':
                    tokens.Add(new Token(TokenType.Power, "^"));
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, ","));
                    break;
                case '!':
                    tokens.Add(new Token(TokenType.Factorial, "!"));
                    break;
                case '%':
                    tokens.Add(new Token(TokenType.Percent, "%"));
                    break;
                case '²': // ²
                    tokens.Add(new Token(TokenType.Square, "²"));
                    break;
                case '³': // ³
                    tokens.Add(new Token(TokenType.Cube, "³"));
                    break;
                case '√': // √
                    tokens.Add(new Token(TokenType.Identifier, "sqrt"));
                    break;
                case '∛': // ∛
                    tokens.Add(new Token(TokenType.Identifier, "cbrt"));
                    break;
                case 'π': // π
                    tokens.Add(new Token(TokenType.Identifier, "pi"));
                    break;
                default:
                    throw new CalculatorSyntaxException($"Unexpected character '{ch}'.");
            }

            i++;
        }

        tokens.Add(new Token(TokenType.End, string.Empty));
        return tokens;
    }
}
