using System;
using System.Text;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents an element in <see cref="SortExpression" />.
    /// </summary>
    [PublicAPI]
    public class SortElementExpression : QueryExpression
    {
        public ResourceFieldChainExpression TargetAttribute { get; }
        public CountExpression Count { get; }
        public bool IsAscending { get; }

        public SortElementExpression(ResourceFieldChainExpression targetAttribute, bool isAscending)
        {
            ArgumentGuard.NotNull(targetAttribute, nameof(targetAttribute));

            TargetAttribute = targetAttribute;
            IsAscending = isAscending;
        }

        public SortElementExpression(CountExpression count, in bool isAscending)
        {
            ArgumentGuard.NotNull(count, nameof(count));

            Count = count;
            IsAscending = isAscending;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSortElement(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (!IsAscending)
            {
                builder.Append('-');
            }

            if (TargetAttribute != null)
            {
                builder.Append(TargetAttribute);
            }
            else if (Count != null)
            {
                builder.Append(Count);
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

            var other = (SortElementExpression)obj;

            return Equals(TargetAttribute, other.TargetAttribute) && Equals(Count, other.Count) && IsAscending == other.IsAscending;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetAttribute, Count, IsAscending);
        }
    }
}
