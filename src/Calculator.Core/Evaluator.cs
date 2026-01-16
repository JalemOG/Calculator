using System;

namespace Calculator.Core
{
    public static class ExpressionEvaluator
    {
        public static int Evaluate(ExpressionNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            // Caso 1: Nodo número
            if (node is ValueNode valueNode)
            {
                return valueNode.Value;
            }

            // Caso 2: Operador unario
            if (node is UnaryOperatorNode unaryNode)
            {
                int operandValue = Evaluate(unaryNode.Operand);
                return ApplyUnaryOperator(unaryNode.Operator, operandValue);
            }

            // Caso 3: Operador binario
            if (node is BinaryOperatorNode binaryNode)
            {
                int leftValue = Evaluate(binaryNode.Left);
                int rightValue = Evaluate(binaryNode.Right);
                return ApplyBinaryOperator(binaryNode.Operator, leftValue, rightValue);
            }

            throw new InvalidOperationException("Tipo de nodo desconocido.");
        }

        private static int ApplyUnaryOperator(string operatorSymbol, int operand)
        {
            return operatorSymbol switch
            {
                "~"   => ~operand,     // NOT bitwise
                "NEG" => -operand,     // menos unario
                _     => throw new InvalidOperationException($"Operador unario no soportado: {operatorSymbol}")
            };
        }

        private static int ApplyBinaryOperator(string operatorSymbol, int left, int right)
        {
            return operatorSymbol switch
            {
                "+"  => left + right,
                "-"  => left - right,
                "*"  => left * right,
                "/"  => right != 0
                            ? left / right
                            : throw new DivideByZeroException("División por cero."),
                "%"  => right != 0
                            ? left % right
                            : throw new DivideByZeroException("Módulo por cero."),
                "**" => (int)Math.Pow(left, right),

                "&"  => left & right,
                "|"  => left | right,
                "^"  => left ^ right,

                _    => throw new InvalidOperationException($"Operador binario no soportado: {operatorSymbol}")
            };
        }
    }
}
