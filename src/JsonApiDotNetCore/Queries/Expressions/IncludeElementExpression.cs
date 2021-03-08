using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents an element in <see cref="IncludeExpression" />.
    /// </summary>
    [PublicAPI]
    public class IncludeElementExpression : QueryExpression
    {
        public RelationshipAttribute Relationship { get; }
        public IReadOnlyCollection<IncludeElementExpression> Children { get; }

        public IncludeElementExpression(RelationshipAttribute relationship)
            : this(relationship, Array.Empty<IncludeElementExpression>())
        {
        }

        public IncludeElementExpression(RelationshipAttribute relationship, IReadOnlyCollection<IncludeElementExpression> children)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(children, nameof(children));

            Relationship = relationship;
            Children = children;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitIncludeElement(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Relationship);

            if (Children.Any())
            {
                builder.Append('{');
                builder.Append(string.Join(",", Children.Select(child => child.ToString())));
                builder.Append('}');
            }

            return builder.ToString();
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

            var other = (IncludeElementExpression)obj;

            return Relationship.Equals(other.Relationship) == Children.SequenceEqual(other.Children);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Relationship);

            foreach (IncludeElementExpression child in Children)
            {
                hashCode.Add(child);
            }

            return hashCode.ToHashCode();
        }
    }
}
