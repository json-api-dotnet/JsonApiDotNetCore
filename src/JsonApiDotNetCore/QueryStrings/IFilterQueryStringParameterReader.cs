using JsonApiDotNetCore.Queries;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Reads the 'filter' query string parameter and produces a set of query constraints from it.
    /// </summary>
    public interface IFilterQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }
}
