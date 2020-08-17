using JsonApiDotNetCore.Queries;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Reads the 'sort' query string parameter and produces a set of query constraints from it.
    /// </summary>
    public interface ISortQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }
}
