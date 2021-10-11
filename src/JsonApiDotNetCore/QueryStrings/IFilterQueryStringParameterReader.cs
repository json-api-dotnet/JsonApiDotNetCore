using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Reads the 'filter' query string parameter and produces a set of query constraints from it.
    /// </summary>
    [PublicAPI]
    public interface IFilterQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }
}
