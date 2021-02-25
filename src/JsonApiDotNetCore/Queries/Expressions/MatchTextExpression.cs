using System;
using System.Text;
using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a text-matching filter function, resulting from text such as: startsWith(name,'A')
    /// </summary>
    [PublicAPI]
    public class MatchTextExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetAttribute { get; }
        public LiteralConstantExpression TextValue { get; }
        public TextMatchKind MatchKind { get; }

        public MatchTextExpression(ResourceFieldChainExpression targetAttribute, LiteralConstantExpression textValue, TextMatchKind matchKind)
        {
            ArgumentGuard.NotNull(targetAttribute, nameof(targetAttribute));
            ArgumentGuard.NotNull(textValue, nameof(textValue));

            TargetAttribute = targetAttribute;
            TextValue = textValue;
            MatchKind = matchKind;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitMatchText(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(MatchKind.ToString().Camelize());
            builder.Append('(');
            builder.Append(string.Join(",", TargetAttribute, TextValue));
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

            var other = (MatchTextExpression)obj;

            return TargetAttribute.Equals(other.TargetAttribute) && TextValue.Equals(other.TextValue) && MatchKind == other.MatchKind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetAttribute, TextValue, MatchKind);
        }
    }
}
