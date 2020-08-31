using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the constant <c>null</c>, resulting from text such as: equals(lastName,null)
    /// </summary>
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
