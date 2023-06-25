using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources.Internal;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.FilterValueConversion;

internal sealed class RelativeTimeFilterValueConverter : IFilterValueConverter
{
    private readonly ISystemClock _systemClock;

    public RelativeTimeFilterValueConverter(ISystemClock systemClock)
    {
        _systemClock = systemClock;
    }

    public bool CanConvert(AttrAttribute attribute)
    {
        return attribute.Type.ClrType == typeof(Reminder) && attribute.Property.PropertyType == typeof(DateTime);
    }

    public object Convert(AttrAttribute attribute, string value, int position, Type outerExpressionType)
    {
        // A leading +/- indicates a relative value, based on the current time.

        if (value.Length > 1 && value[0] is '+' or '-')
        {
            if (outerExpressionType != typeof(ComparisonExpression))
            {
                throw new QueryParseException("A relative time can only be used in a comparison function.", position);
            }

            var timeSpan = ConvertStringValueTo<TimeSpan>(value[1..], position);
            TimeSpan offset = value[0] == '-' ? -timeSpan : timeSpan;
            return new TimeRange(_systemClock.UtcNow.UtcDateTime, offset);
        }

        return ConvertStringValueTo<DateTime>(value, position);
    }

    private static T ConvertStringValueTo<T>(string value, int position)
    {
        try
        {
            return (T)RuntimeTypeConverter.ConvertType(value, typeof(T))!;
        }
        catch (FormatException exception)
        {
            throw new QueryParseException($"Failed to convert '{value}' of type 'String' to type '{typeof(T).Name}'.", position, exception);
        }
    }
}
