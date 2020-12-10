using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a lookup table of sparse fieldsets per resource type.
    /// </summary>
    public class SparseFieldTableExpression : QueryExpression
    {
        public IReadOnlyDictionary<ResourceContext, SparseFieldSetExpression> Table { get; }

        public SparseFieldTableExpression(IReadOnlyDictionary<ResourceContext, SparseFieldSetExpression> table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));

            if (!table.Any())
            {
                throw new ArgumentException("Must have one or more entries.", nameof(table));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSparseFieldTable(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var (resource, fields) in Table)
            {
                if (builder.Length > 0)
                {
                    builder.Append(",");
                }

                builder.Append(resource.PublicName);
                builder.Append("(");
                builder.Append(fields);
                builder.Append(")");
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

            var other = (SparseFieldTableExpression) obj;

            return Table.SequenceEqual(other.Table);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var (resourceContext, sparseFieldSet) in Table)
            {
                hashCode.Add(resourceContext);
                hashCode.Add(sparseFieldSet);
            }

            return hashCode.ToHashCode();
        }
    }
}
