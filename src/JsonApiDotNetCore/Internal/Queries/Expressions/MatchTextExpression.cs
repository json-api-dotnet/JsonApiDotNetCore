using System;
using System.Text;
using Humanizer;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class MatchTextExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetAttribute { get; }
        public LiteralConstantExpression TextValue { get; }
        public TextMatchKind MatchKind { get; }

        public MatchTextExpression(ResourceFieldChainExpression targetAttribute, LiteralConstantExpression textValue,
            TextMatchKind matchKind)
        {
            TargetAttribute = targetAttribute ?? throw new ArgumentNullException(nameof(targetAttribute));
            TextValue = textValue ?? throw new ArgumentNullException(nameof(textValue));
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
    }
}
