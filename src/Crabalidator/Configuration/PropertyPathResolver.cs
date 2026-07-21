using System.Linq.Expressions;

namespace Crabalidator.Configuration
{
    internal static class PropertyPathResolver
    {
        public static PropertyPath Resolve(LambdaExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var members = new Stack<string>();
            var current = StripConvert(expression.Body);

            while (current is MemberExpression member)
            {
                members.Push(member.Member.Name);
                current = StripConvert(member.Expression);
            }

            if (members.Count == 0 || current != expression.Parameters[0])
            {
                throw new ArgumentException("RuleFor only supports direct member access expressions.", nameof(expression));
            }

            var path = string.Join(".", members);
            return new PropertyPath(members.Last(), path);
        }

        private static Expression StripConvert(Expression expression)
        {
            while (expression is UnaryExpression unary
                && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
            {
                expression = unary.Operand;
            }

            return expression;
        }
    }
}
