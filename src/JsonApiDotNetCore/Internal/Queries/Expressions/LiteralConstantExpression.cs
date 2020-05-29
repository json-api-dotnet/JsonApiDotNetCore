using System;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class LiteralConstantExpression : IdentifierExpression
    {
        public string Value { get; }

        public LiteralConstantExpression(string text)
        {
            Value = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitLiteralConstant(this, argument);
        }

        public override string ToString()
        {
            string value = Value.Replace("\'", "\'\'");
            return $"'{value}'";
        }
    }
}
