namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class PaginationElementQueryStringValueExpression : QueryExpression
    {
        public ResourceFieldChainExpression Scope { get; }
        public int Value { get; }

        public PaginationElementQueryStringValueExpression(ResourceFieldChainExpression scope, int value)
        {
            Scope = scope;
            Value = value;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.PaginationElementQueryStringValue(this, argument);
        }

        public override string ToString()
        {
            return Scope == null ? Value.ToString() : $"{Scope}: {Value}";
        }
    }
}
