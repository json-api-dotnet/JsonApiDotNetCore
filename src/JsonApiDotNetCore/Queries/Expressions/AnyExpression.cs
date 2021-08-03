using System;
using System.Collections.Immutable;
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
    public class AnyExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetAttribute { get; }
        public IImmutableSet<LiteralConstantExpression> Constants { get; }

        public AnyExpression(ResourceFieldChainExpression targetAttribute, IImmutableSet<LiteralConstantExpression> constants)
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
            return visitor.VisitAny(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Keywords.Any);
            builder.Append('(');
            builder.Append(TargetAttribute);
            builder.Append(',');
            builder.Append(string.Join(",", Constants.Select(constant => constant.ToString()).OrderBy(value => value)));
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

            var other = (AnyExpression)obj;

            return TargetAttribute.Equals(other.TargetAttribute) && Constants.SetEquals(other.Constants);
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
