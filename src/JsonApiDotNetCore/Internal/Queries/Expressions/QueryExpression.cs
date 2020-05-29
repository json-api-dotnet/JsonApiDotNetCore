namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public abstract class QueryExpression
    {
        public abstract TResult
            Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument);
    }
}
