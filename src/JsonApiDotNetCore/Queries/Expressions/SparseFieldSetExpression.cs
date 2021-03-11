using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a sparse fieldset, resulting from text such as: firstName,lastName,articles
    /// </summary>
    [PublicAPI]
    public class SparseFieldSetExpression : QueryExpression
    {
        public IReadOnlyCollection<ResourceFieldAttribute> Fields { get; }

        public SparseFieldSetExpression(IReadOnlyCollection<ResourceFieldAttribute> fields)
        {
            ArgumentGuard.NotNullNorEmpty(fields, nameof(fields));

            Fields = fields;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSparseFieldSet(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Fields.Select(child => child.PublicName));
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

            var other = (SparseFieldSetExpression)obj;

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
