using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.QueryStrings;

/// <summary>
/// Provides conversion of a single-quoted value that occurs in a filter function of a query string.
/// </summary>
[PublicAPI]
public interface IFilterValueConverter
{
    /// <summary>
    /// Indicates whether this converter can be used for the specified <see cref="AttrAttribute" />.
    /// </summary>
    /// <param name="attribute">
    /// The JSON:API attribute this conversion applies to.
    /// </param>
    bool CanConvert(AttrAttribute attribute);

    /// <summary>
    /// Converts <paramref name="value" /> to the specified <paramref name="attribute" />.
    /// </summary>
    /// <param name="attribute">
    /// The JSON:API attribute this conversion applies to.
    /// </param>
    /// <param name="value">
    /// The literal text (without the surrounding single quotes) from the query string.
    /// </param>
    /// <param name="outerExpressionType">
    /// The filter function this conversion applies to, which can be <see cref="ComparisonExpression" />, <see cref="AnyExpression" /> or
    /// <see cref="MatchTextExpression" />.
    /// </param>
    /// <returns>
    /// The converted value. Must not be null. In case the type differs from the resource property type, use a
    /// <see cref="QueryExpressionRewriter{TArgument}" /> from <see cref="IResourceDefinition{TResource,TId}.OnApplyFilter" /> to produce a valid filter.
    /// </returns>
    /// <exception cref="QueryParseException">
    /// The conversion failed because <paramref name="value" /> is invalid.
    /// </exception>
    object Convert(AttrAttribute attribute, string value, Type outerExpressionType);
}
