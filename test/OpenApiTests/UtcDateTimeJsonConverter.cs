namespace OpenApiTests;

// Used by client libraries only, to enforce that sent DateTime values are always in UTC.
public sealed class UtcDateTimeJsonConverter()
    : ValueTypeJsonConverter<DateTime>(value => DateTimeOffset.Parse(value).UtcDateTime, value => value.ToUniversalTime().ToString("O"));
