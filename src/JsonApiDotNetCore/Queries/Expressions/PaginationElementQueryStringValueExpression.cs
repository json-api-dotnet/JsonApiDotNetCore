using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents an element in <see cref="PaginationQueryStringValueExpression" />.
    /// </summary>
    [PublicAPI]
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (PaginationElementQueryStringValueExpression)obj;

            return Equals(Scope, other.Scope) && Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Scope, Value);
        }
    }
}
