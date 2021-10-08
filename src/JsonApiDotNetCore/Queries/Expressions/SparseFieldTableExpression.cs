using System;
using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a lookup table of sparse fieldsets per resource type.
    /// </summary>
    [PublicAPI]
    public class SparseFieldTableExpression : QueryExpression
    {
        public IImmutableDictionary<ResourceType, SparseFieldSetExpression> Table { get; }

        public SparseFieldTableExpression(IImmutableDictionary<ResourceType, SparseFieldSetExpression> table)
        {
            ArgumentGuard.NotNullNorEmpty(table, nameof(table), "entries");

            Table = table;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSparseFieldTable(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach ((ResourceType resource, SparseFieldSetExpression fields) in Table)
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }

                builder.Append(resource.PublicName);
                builder.Append('(');
                builder.Append(fields);
                builder.Append(')');
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

            var other = (SparseFieldTableExpression)obj;

            return Table.DictionaryEqual(other.Table);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach ((ResourceType resourceType, SparseFieldSetExpression sparseFieldSet) in Table)
            {
                hashCode.Add(resourceType);
                hashCode.Add(sparseFieldSet);
            }

            return hashCode.ToHashCode();
        }
    }
}
