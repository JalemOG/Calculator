using System;
using System.Collections.Generic;

namespace Calculator.Core
{
    // Nodo base del árbol
    public abstract class ExpressionNode
    {
    }

    // Nodo hoja (número)
    public sealed class ValueNode : ExpressionNode
    {
        public int Value { get; }

        public ValueNode(int value)
        {
            Value = value;
        }
    }

    // Nodo operador unario (ej: ~, NEG)
    public sealed class UnaryOperatorNode : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Operand { get; }

        public UnaryOperatorNode(string operatorSymbol, ExpressionNode operand)
        {
            Operator = operatorSymbol;
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }
    }

    // Nodo operador binario (ej: +, *, **)
    public sealed class BinaryOperatorNode : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }

        public BinaryOperatorNode(string operatorSymbol, ExpressionNode left, ExpressionNode right)
        {
            Operator = operatorSymbol;
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }
    }

    // Constructor del árbol a partir de postfix
    public static class ExpressionTreeBuilder
    {
        public static ExpressionNode BuildFromPostfix(List<Token> postfixTokens)
        {
            if (postfixTokens == null)
                throw new ArgumentNullException(nameof(postfixTokens));

            var nodeStack = new Stack<ExpressionNode>();

            foreach (var token in postfixTokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        nodeStack.Push(new ValueNode(token.IntValue!.Value));
                        break;

                    case TokenType.Operator:
                        if (IsUnaryOperator(token.Text))
                        {
                            if (nodeStack.Count < 1)
                                throw new FormatException($"Operador unario '{token.Text}' sin operando.");

                            var operand = nodeStack.Pop();
                            nodeStack.Push(new UnaryOperatorNode(token.Text, operand));
                        }
                        else
                        {
                            if (nodeStack.Count < 2)
                                throw new FormatException($"Operador binario '{token.Text}' sin operandos suficientes.");

                            var right = nodeStack.Pop();
                            var left = nodeStack.Pop();
                            nodeStack.Push(new BinaryOperatorNode(token.Text, left, right));
                        }
                        break;

                    default:
                        throw new FormatException($"Token no válido en postfix: {token.Type}");
                }
            }

            if (nodeStack.Count != 1)
                throw new FormatException("La expresión postfix es inválida.");

            return nodeStack.Pop();
        }

        private static bool IsUnaryOperator(string operatorSymbol)
        {
            return operatorSymbol == "~" || operatorSymbol == "NEG";
        }
    }
}
