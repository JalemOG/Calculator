namespace Calculator.Core
{
    public static class ExpressionEngine
    {
        public static int EvaluateExpression(string expression)
        {
            var tokens = Tokenizer.Tokenize(expression);
            var postfix = ShuntingYard.ConvertToPostfix(tokens);
            var tree = ExpressionTreeBuilder.BuildFromPostfix(postfix);
            return ExpressionEvaluator.Evaluate(tree);
        }
    }
}
