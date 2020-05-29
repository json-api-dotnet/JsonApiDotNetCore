using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class ResourceFieldChainExpression : IdentifierExpression, IEquatable<ResourceFieldChainExpression>
    {
        public IReadOnlyCollection<ResourceFieldAttribute> Fields { get; }

        public ResourceFieldChainExpression(ResourceFieldAttribute field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            Fields = new[] {field};
        }

        public ResourceFieldChainExpression(IReadOnlyCollection<ResourceFieldAttribute> fields)
        {
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));

            if (!fields.Any())
            {
                throw new ArgumentException("Must have one or more fields.", nameof(fields));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor,
            TArgument argument)
        {
            return visitor.VisitResourceFieldChain(this, argument);
        }

        public override string ToString()
        {
            return string.Join(".", Fields.Select(field => field.PublicName));
        }

        public bool Equals(ResourceFieldChainExpression other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Fields.SequenceEqual(other.Fields);
        }

        public override bool Equals(object other)
        {
            return Equals(other as ResourceFieldChainExpression);
        }

        public override int GetHashCode()
        {
            return Fields.Aggregate(0, HashCode.Combine);
        }
    }
}
