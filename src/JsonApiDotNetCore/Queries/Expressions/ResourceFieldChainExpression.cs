using System;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a chain of fields (relationships and attributes), resulting from text such as: articles.revisions.author
    /// </summary>
    [PublicAPI]
    public class ResourceFieldChainExpression : IdentifierExpression
    {
        public IImmutableList<ResourceFieldAttribute> Fields { get; }

        public ResourceFieldChainExpression(ResourceFieldAttribute field)
        {
            ArgumentGuard.NotNull(field, nameof(field));

            Fields = ImmutableArray.Create(field);
        }

        public ResourceFieldChainExpression(IImmutableList<ResourceFieldAttribute> fields)
        {
            ArgumentGuard.NotNullNorEmpty(fields, nameof(fields));

            Fields = fields;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitResourceFieldChain(this, argument);
        }

        public override string ToString()
        {
            return string.Join(".", Fields.Select(field => field.PublicName));
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

            var other = (ResourceFieldChainExpression)obj;

            return Fields.SequenceEqual(other.Fields);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (ResourceFieldAttribute field in Fields)
            {
                hashCode.Add(field);
            }

            return hashCode.ToHashCode();
        }
    }
}
