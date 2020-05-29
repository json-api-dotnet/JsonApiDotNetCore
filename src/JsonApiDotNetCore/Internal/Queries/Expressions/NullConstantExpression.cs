using JsonApiDotNetCore.Internal.Queries.Parsing;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class NullConstantExpression : IdentifierExpression
    {
        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitNullConstant(this, argument);
        }

        public override string ToString()
        {
            return Keywords.Null;
        }
    }
}
