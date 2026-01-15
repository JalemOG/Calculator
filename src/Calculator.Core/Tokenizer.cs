using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calculator.Core
{
    // 1) Tipos de token que reconocemos
    public enum TokenType
    {
        Number,
        Operator,
        LeftParen,
        RightParen
    }

    // 2) Representación de un token
    public sealed class Token
    {
        public TokenType Type { get; }
        public string Text { get; }        // Texto literal del token (ej: "12", "+", "**", "(")
        public int? IntValue { get; }      // Solo aplica si Type == Number

        private Token(TokenType type, string text, int? intValue = null)
        {
            Type = type;
            Text = text;
            IntValue = intValue;
        }

        public static Token Number(int value) =>
            new Token(TokenType.Number, value.ToString(CultureInfo.InvariantCulture), value);

        public static Token Op(string op) =>
            new Token(TokenType.Operator, op);

        public static Token LParen() =>
            new Token(TokenType.LeftParen, "(");

        public static Token RParen() =>
            new Token(TokenType.RightParen, ")");

        public override string ToString()
        {
            return Type switch
            {
                TokenType.Number     => $"Number({IntValue})",
                TokenType.Operator   => $"Op({Text})",
                TokenType.LeftParen  => "LPAREN",
                TokenType.RightParen => "RPAREN",
                _                    => Text
            };
        }
    }

    // 3) Tokenizer: convierte un string en lista de Tokens
    public static class Tokenizer
    {
        // Operadores soportados por el proyecto (símbolos).
        // NOTA: "**" se maneja aparte porque son 2 caracteres.
        private static readonly HashSet<string> Operators = new HashSet<string>
        {
            "+", "-", "*", "/", "%", "**", "&", "|", "^", "~"
        };

        public static List<Token> Tokenize(string expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var tokens = new List<Token>();
            int i = 0;

            while (i < expression.Length)
            {
                char c = expression[i];

                // 1) Ignorar espacios
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // 2) Paréntesis
                if (c == '(')
                {
                    tokens.Add(Token.LParen());
                    i++;
                    continue;
                }

                if (c == ')')
                {
                    tokens.Add(Token.RParen());
                    i++;
                    continue;
                }

                // 3) Números enteros (secuencia de dígitos)
                if (char.IsDigit(c))
                {
                    int start = i;

                    while (i < expression.Length && char.IsDigit(expression[i]))
                        i++;

                    string numberText = expression.Substring(start, i - start);

                    if (!int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
                        throw new FormatException($"Número inválido o fuera de rango: '{numberText}'.");

                    tokens.Add(Token.Number(value));
                    continue;
                }

                // 4) Operadores
                // Caso especial: "**"
                if (c == '*' && i + 1 < expression.Length && expression[i + 1] == '*')
                {
                    tokens.Add(Token.Op("**"));
                    i += 2;
                    continue;
                }

                // Operadores de 1 caracter
                string op1 = c.ToString();
                if (Operators.Contains(op1))
                {
                    tokens.Add(Token.Op(op1));
                    i++;
                    continue;
                }

                // 5) Caracter no reconocido
                throw new FormatException($"Caracter no soportado en posición {i}: '{c}'.");
            }

            return tokens;
        }
    }
}
