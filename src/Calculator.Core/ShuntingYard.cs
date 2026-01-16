using System;
using System.Collections.Generic;

namespace Calculator.Core
{
    public enum Associativity
    {
        Left,
        Right
    }

    public sealed class OperatorInfo
    {
        public string Symbol { get; }
        public int Precedence { get; }
        public Associativity Associativity { get; }
        public int Arity { get; } // 1 = unario, 2 = binario

        public OperatorInfo(string symbol, int precedence, Associativity associativity, int arity)
        {
            Symbol = symbol;
            Precedence = precedence;
            Associativity = associativity;
            Arity = arity;
        }
    }

    public static class ShuntingYard
    {
        // Tabla central de operadores del lenguaje
        private static readonly Dictionary<string, OperatorInfo> OperatorTable = new()
        {
            // Unarios (mayor precedencia)
            { "~",   new OperatorInfo("~",   7, Associativity.Right, 1) },
            { "NEG", new OperatorInfo("NEG", 7, Associativity.Right, 1) },

            // Potencia
            { "**",  new OperatorInfo("**",  6, Associativity.Right, 2) },

            // Multiplicativos
            { "*",   new OperatorInfo("*",   5, Associativity.Left,  2) },
            { "/",   new OperatorInfo("/",   5, Associativity.Left,  2) },
            { "%",   new OperatorInfo("%",   5, Associativity.Left,  2) },

            // Aditivos
            { "+",   new OperatorInfo("+",   4, Associativity.Left,  2) },
            { "-",   new OperatorInfo("-",   4, Associativity.Left,  2) },

            // Bitwise
            { "&",   new OperatorInfo("&",   3, Associativity.Left,  2) },
            { "^",   new OperatorInfo("^",   2, Associativity.Left,  2) },
            { "|",   new OperatorInfo("|",   1, Associativity.Left,  2) },
        };

        public static List<Token> ConvertToPostfix(List<Token> infixTokens)
        {
            if (infixTokens == null)
                throw new ArgumentNullException(nameof(infixTokens));

            var normalizedTokens = HandleUnaryMinus(infixTokens);

            var postfixTokens = new List<Token>();
            var operatorStack = new Stack<Token>();

            foreach (var token in normalizedTokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        postfixTokens.Add(token);
                        break;

                    case TokenType.Operator:
                        if (!OperatorTable.TryGetValue(token.Text, out var currentOperator))
                            throw new FormatException($"Operador no soportado: '{token.Text}'.");

                        while (operatorStack.Count > 0 &&
                               operatorStack.Peek().Type == TokenType.Operator)
                        {
                            var topOperatorToken = operatorStack.Peek();

                            if (!OperatorTable.TryGetValue(topOperatorToken.Text, out var stackOperator))
                                throw new FormatException($"Operador no soportado: '{topOperatorToken.Text}'.");

                            bool higherPrecedence =
                                stackOperator.Precedence > currentOperator.Precedence;

                            bool samePrecedenceLeftAssociative =
                                stackOperator.Precedence == currentOperator.Precedence &&
                                currentOperator.Associativity == Associativity.Left;

                            if (higherPrecedence || samePrecedenceLeftAssociative)
                                postfixTokens.Add(operatorStack.Pop());
                            else
                                break;
                        }

                        operatorStack.Push(token);
                        break;

                    case TokenType.LeftParen:
                        operatorStack.Push(token);
                        break;

                    case TokenType.RightParen:
                        bool leftParenFound = false;

                        while (operatorStack.Count > 0)
                        {
                            var top = operatorStack.Pop();

                            if (top.Type == TokenType.LeftParen)
                            {
                                leftParenFound = true;
                                break;
                            }

                            postfixTokens.Add(top);
                        }

                        if (!leftParenFound)
                            throw new FormatException("Paréntesis desbalanceados.");

                        break;

                    default:
                        throw new FormatException($"Tipo de token no soportado: {token.Type}");
                }
            }

            while (operatorStack.Count > 0)
            {
                var remaining = operatorStack.Pop();

                if (remaining.Type == TokenType.LeftParen)
                    throw new FormatException("Paréntesis desbalanceados.");

                postfixTokens.Add(remaining);
            }

            return postfixTokens;
        }

        private static List<Token> HandleUnaryMinus(List<Token> tokens)
        {
            var result = new List<Token>(tokens.Count);

            for (int i = 0; i < tokens.Count; i++)
            {
                var current = tokens[i];

                if (current.Type == TokenType.Operator && current.Text == "-")
                {
                    bool isUnary =
                        i == 0 ||
                        tokens[i - 1].Type == TokenType.LeftParen ||
                        tokens[i - 1].Type == TokenType.Operator;

                    if (isUnary)
                    {
                        result.Add(Token.Op("NEG"));
                        continue;
                    }
                }

                result.Add(current);
            }

            return result;
        }
    }
}
