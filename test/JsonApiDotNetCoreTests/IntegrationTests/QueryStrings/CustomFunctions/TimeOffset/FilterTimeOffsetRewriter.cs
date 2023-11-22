using JsonApiDotNetCore.Queries.Expressions;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

internal sealed class FilterTimeOffsetRewriter : QueryExpressionRewriter<object?>
{
    private static readonly Dictionary<ComparisonOperator, ComparisonOperator> InverseComparisonOperatorTable = new()
    {
        [ComparisonOperator.GreaterThan] = ComparisonOperator.LessThan,
        [ComparisonOperator.GreaterOrEqual] = ComparisonOperator.LessOrEqual,
        [ComparisonOperator.Equals] = ComparisonOperator.Equals,
        [ComparisonOperator.LessThan] = ComparisonOperator.GreaterThan,
        [ComparisonOperator.LessOrEqual] = ComparisonOperator.GreaterOrEqual
    };

    private readonly ISystemClock _systemClock;

    public FilterTimeOffsetRewriter(ISystemClock systemClock)
    {
        _systemClock = systemClock;
    }

    public override QueryExpression? VisitComparison(ComparisonExpression expression, object? argument)
    {
        if (expression.Right is TimeOffsetExpression timeOffset)
        {
            DateTime currentTime = _systemClock.UtcNow.UtcDateTime;

            var offsetComparison =
                new ComparisonExpression(timeOffset.Value < TimeSpan.Zero ? InverseComparisonOperatorTable[expression.Operator] : expression.Operator,
                    expression.Left, new LiteralConstantExpression(currentTime + timeOffset.Value));

            ComparisonExpression? timeComparison = expression.Operator is ComparisonOperator.LessThan or ComparisonOperator.LessOrEqual
                ? new ComparisonExpression(timeOffset.Value < TimeSpan.Zero ? ComparisonOperator.LessOrEqual : ComparisonOperator.GreaterOrEqual,
                    expression.Left, new LiteralConstantExpression(currentTime))
                : null;

            return timeComparison == null ? offsetComparison : new LogicalExpression(LogicalOperator.And, offsetComparison, timeComparison);
        }

        return base.VisitComparison(expression, argument);
    }
}
