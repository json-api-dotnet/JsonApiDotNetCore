namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Converts a constant value within a query string parameter to an <see cref="object" />.
/// </summary>
/// <param name="value">
/// The constant value to convert from.
/// </param>
/// <param name="position">
/// The zero-based position of <paramref name="value" /> in the query string parameter value.
/// </param>
/// <returns>
/// The converted object instance.
/// </returns>
public delegate object ConstantValueConverter(string value, int position);
