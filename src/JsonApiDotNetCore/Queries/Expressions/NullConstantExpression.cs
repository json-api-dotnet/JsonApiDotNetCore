using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the constant <c>null</c>, resulting from text such as: equals(lastName,null)
    /// </summary>
    [PublicAPI]
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

            return true;
        }

        public override int GetHashCode()
        {
            return new HashCode().ToHashCode();
        }
    }
}
