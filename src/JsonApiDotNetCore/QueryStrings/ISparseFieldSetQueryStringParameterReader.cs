using JsonApiDotNetCore.Queries;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Reads the 'fields' query string parameter and produces a set of query constraints from it.
    /// </summary>
    public interface ISparseFieldSetQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }
}
