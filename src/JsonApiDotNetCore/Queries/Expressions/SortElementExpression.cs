using System;
using System.Text;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents an element in <see cref="SortExpression"/>.
    /// </summary>
    public class SortElementExpression : QueryExpression
    {
        public ResourceFieldChainExpression TargetAttribute { get; }
        public CountExpression Count { get; }
        public bool IsAscending { get; }

        public SortElementExpression(ResourceFieldChainExpression targetAttribute, bool isAscending)
        {
            TargetAttribute = targetAttribute ?? throw new ArgumentNullException(nameof(targetAttribute));
            IsAscending = isAscending;
        }

        public SortElementExpression(CountExpression count, in bool isAscending)
        {
            Count = count ?? throw new ArgumentNullException(nameof(count));
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
    }
}
