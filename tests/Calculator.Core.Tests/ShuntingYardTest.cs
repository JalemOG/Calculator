using System.Linq;
using Calculator.Core;
using Xunit;

namespace Calculator.Core.Tests
{
    public class ShuntingYardTests
    {
        private static string[] ToSimplePostfix(string expression)
        {
            var tokens = Tokenizer.Tokenize(expression);
            var postfix = ShuntingYard.ConvertToPostfix(tokens);
            return postfix.Select(t => t.Type == TokenType.Number ? t.IntValue!.Value.ToString() : t.Text).ToArray();
        }

        [Fact]
        public void ConvertToPostfix_RespectsParenthesesAndPrecedence()
        {
            var postfix = ToSimplePostfix("(5*7)+(12/6)");
            Assert.Equal(new[] { "5", "7", "*", "12", "6", "/", "+" }, postfix);
        }

        [Fact]
        public void ConvertToPostfix_PowerIsRightAssociative()
        {
            // 2 ** (3 ** 2) => postfix: 2 3 2 ** **
            var postfix = ToSimplePostfix("2**3**2");
            Assert.Equal(new[] { "2", "3", "2", "**", "**" }, postfix);
        }

        [Fact]
        public void ConvertToPostfix_HandlesUnaryMinusAsNEG()
        {
            // -(5+3)*4 => 5 3 + NEG 4 *
            var postfix = ToSimplePostfix("-(5+3)*4");
            Assert.Equal(new[] { "5", "3", "+", "NEG", "4", "*" }, postfix);
        }
    }
}
