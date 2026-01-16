using System;
using Calculator.Core;
using Xunit;

namespace Calculator.Core.Tests
{
    public class EvaluatorTests
    {
        private static int EvaluateExpression(string expression)
        {
            var tokens = Tokenizer.Tokenize(expression);
            var postfix = ShuntingYard.ConvertToPostfix(tokens);
            var tree = ExpressionTreeBuilder.BuildFromPostfix(postfix);
            return ExpressionEvaluator.Evaluate(tree);
        }

        [Fact]
        public void Evaluate_ArithmeticExample_ReturnsExpected()
        {
            Assert.Equal(37, EvaluateExpression("(5*7)+(12/6)"));
        }

        [Fact]
        public void Evaluate_UnaryMinus_Works()
        {
            Assert.Equal(-32, EvaluateExpression("-(5+3)*4"));
        }

        [Fact]
        public void Evaluate_BitwiseOperators_Work()
        {
            // 5 & 3 = 1, 1 | 1 = 1
            Assert.Equal(1, EvaluateExpression("(5&3)|1"));
            Assert.Equal(6, EvaluateExpression("5^3")); // XOR
        }

        [Fact]
        public void Evaluate_NotOperator_Works()
        {
            Assert.Equal(~5, EvaluateExpression("~5"));
        }

        [Fact]
        public void Evaluate_DivisionByZero_Throws()
        {
            Assert.Throws<DivideByZeroException>(() => EvaluateExpression("5/0"));
        }
    }
}
