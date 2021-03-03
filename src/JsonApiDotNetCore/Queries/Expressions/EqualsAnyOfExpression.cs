using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the "any" filter function, resulting from text such as: any(name,'Jack','Joe')
    /// </summary>
    [PublicAPI]
    public class EqualsAnyOfExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetAttribute { get; }
        public IReadOnlyCollection<LiteralConstantExpression> Constants { get; }

        public EqualsAnyOfExpression(ResourceFieldChainExpression targetAttribute, IReadOnlyCollection<LiteralConstantExpression> constants)
        {
            ArgumentGuard.NotNull(targetAttribute, nameof(targetAttribute));
            ArgumentGuard.NotNull(constants, nameof(constants));

            if (constants.Count < 2)
            {
                throw new ArgumentException("At least two constants are required.", nameof(constants));
            }

            TargetAttribute = targetAttribute;
            Constants = constants;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitEqualsAnyOf(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Keywords.Any);
            builder.Append('(');
            builder.Append(TargetAttribute);
            builder.Append(',');
            builder.Append(string.Join(",", Constants.Select(constant => constant.ToString())));
            builder.Append(')');

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

            var other = (EqualsAnyOfExpression)obj;

            return TargetAttribute.Equals(other.TargetAttribute) && Constants.SequenceEqual(other.Constants);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(TargetAttribute);

            foreach (LiteralConstantExpression constant in Constants)
            {
                hashCode.Add(constant);
            }

            return hashCode.ToHashCode();
        }
    }
}
