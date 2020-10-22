using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a sparse fieldset, resulting from text such as: firstName,lastName
    /// </summary>
    public class SparseFieldSetExpression : QueryExpression
    {
        // TODO: Once aspnetcore 5 is released, use IReadOnlySet here and in other places where functionally desired.
        public IReadOnlyCollection<AttrAttribute> Attributes { get; }

        public SparseFieldSetExpression(IReadOnlyCollection<AttrAttribute> attributes)
        {
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));

            if (!attributes.Any())
            {
                throw new ArgumentException("Must have one or more attributes.", nameof(attributes));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSparseFieldSet(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Attributes.Select(child => child.PublicName));
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

            var other = (SparseFieldSetExpression) obj;

            return Attributes.SequenceEqual(other.Attributes);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var attribute in Attributes)
            {
                hashCode.Add(attribute);
            }

            return hashCode.ToHashCode();
        }
    }
}
