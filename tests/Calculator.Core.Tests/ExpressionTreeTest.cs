using System;
using Calculator.Core;
using Xunit;

namespace Calculator.Core.Tests
{
    public class ExpressionTreeTests
    {
        [Fact]
        public void BuildFromPostfix_CreatesBinaryOperatorNodeWithCorrectChildrenOrder()
        {
            // postfix: 5 7 *
            var tokens = Tokenizer.Tokenize("5*7");
            var postfix = ShuntingYard.ConvertToPostfix(tokens);

            var root = ExpressionTreeBuilder.BuildFromPostfix(postfix);

            var binary = Assert.IsType<BinaryOperatorNode>(root);
            Assert.Equal("*", binary.Operator);

            var left = Assert.IsType<ValueNode>(binary.Left);
            var right = Assert.IsType<ValueNode>(binary.Right);

            Assert.Equal(5, left.Value);
            Assert.Equal(7, right.Value);
        }

        [Fact]
        public void BuildFromPostfix_CreatesUnaryOperatorNode()
        {
            // ~5 => postfix: 5 ~
            var tokens = Tokenizer.Tokenize("~5");
            var postfix = ShuntingYard.ConvertToPostfix(tokens);

            var root = ExpressionTreeBuilder.BuildFromPostfix(postfix);

            var unary = Assert.IsType<UnaryOperatorNode>(root);
            Assert.Equal("~", unary.Operator);

            var operand = Assert.IsType<ValueNode>(unary.Operand);
            Assert.Equal(5, operand.Value);
        }

        [Fact]
        public void BuildFromPostfix_InvalidPostfix_Throws()
        {
            // "5 +" en postfix es inválido (faltan operandos)
            var badPostfix = new System.Collections.Generic.List<Token>
            {
                Token.Number(5),
                Token.Op("+")
            };

            Assert.Throws<FormatException>(() => ExpressionTreeBuilder.BuildFromPostfix(badPostfix));
        }
    }
}
