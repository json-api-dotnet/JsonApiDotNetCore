using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

internal sealed class TimeOffsetFilterParser(IResourceFactory resourceFactory)
    : FilterParser(resourceFactory)
{
    protected override bool IsFunction(string name)
    {
        if (name == TimeOffsetExpression.Keyword)
        {
            return true;
        }

        return base.IsFunction(name);
    }

    protected override FunctionExpression ParseFunction()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: TimeOffsetExpression.Keyword })
        {
            return ParseTimeOffset();
        }

        return base.ParseFunction();
    }

    private TimeOffsetExpression ParseTimeOffset()
    {
        EatText(TimeOffsetExpression.Keyword);
        EatSingleCharacterToken(TokenKind.OpenParen);

        LiteralConstantExpression constant = ParseTimeSpanConstant();

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new TimeOffsetExpression(constant);
    }

    private LiteralConstantExpression ParseTimeSpanConstant()
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.QuotedText)
        {
            string value = token.Value!;

            if (value.Length > 1 && value[0] is '+' or '-')
            {
                TimeSpan timeSpan = ConvertStringToTimeSpan(value[1..], position);
                TimeSpan timeOffset = value[0] == '-' ? -timeSpan : timeSpan;

                return new LiteralConstantExpression(timeOffset, value);
            }
        }

        throw new QueryParseException("Time offset between quotes expected.", position);
    }

    private static TimeSpan ConvertStringToTimeSpan(string value, int position)
    {
        try
        {
            return (TimeSpan)RuntimeTypeConverter.ConvertType(value, typeof(TimeSpan))!;
        }
        catch (FormatException exception)
        {
            throw new QueryParseException($"Failed to convert '{value}' of type 'String' to type '{nameof(TimeSpan)}'.", position, exception);
        }
    }

    protected override ComparisonExpression ParseComparison(string operatorName)
    {
        int position = GetNextTokenPositionOrEnd();
        ComparisonExpression comparison = base.ParseComparison(operatorName);

        if (comparison.Left is TimeOffsetExpression)
        {
            throw new QueryParseException("The 'timeOffset' function can only be used at the right side of comparisons.", position);
        }

        return comparison;
    }
}
