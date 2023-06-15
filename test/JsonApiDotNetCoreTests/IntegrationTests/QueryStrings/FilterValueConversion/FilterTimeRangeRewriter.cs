using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.FilterValueConversion;

internal sealed class FilterTimeRangeRewriter : QueryExpressionRewriter<object?>
{
    private static readonly Dictionary<ComparisonOperator, ComparisonOperator> InverseComparisonOperatorTable = new()
    {
        [ComparisonOperator.GreaterThan] = ComparisonOperator.LessThan,
        [ComparisonOperator.GreaterOrEqual] = ComparisonOperator.LessOrEqual,
        [ComparisonOperator.Equals] = ComparisonOperator.Equals,
        [ComparisonOperator.LessThan] = ComparisonOperator.GreaterThan,
        [ComparisonOperator.LessOrEqual] = ComparisonOperator.GreaterOrEqual
    };

    public override QueryExpression? VisitComparison(ComparisonExpression expression, object? argument)
    {
        if (expression.Right is LiteralConstantExpression { TypedValue: TimeRange timeRange })
        {
            var offsetComparison =
                new ComparisonExpression(timeRange.Offset < TimeSpan.Zero ? InverseComparisonOperatorTable[expression.Operator] : expression.Operator,
                    expression.Left, new LiteralConstantExpression(timeRange.Time + timeRange.Offset));

            ComparisonExpression? timeComparison = expression.Operator is ComparisonOperator.LessThan or ComparisonOperator.LessOrEqual
                ? new ComparisonExpression(timeRange.Offset < TimeSpan.Zero ? ComparisonOperator.LessOrEqual : ComparisonOperator.GreaterOrEqual,
                    expression.Left, new LiteralConstantExpression(timeRange.Time))
                : null;

            return timeComparison == null ? offsetComparison : new LogicalExpression(LogicalOperator.And, offsetComparison, timeComparison);
        }

        return base.VisitComparison(expression, argument);
    }
}
