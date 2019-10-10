using System.Text.RegularExpressions;

namespace JsonApiDotNetCore.Query
{
    public abstract class QueryParameterService : IQueryParameterService
    {

        /// <summary>
        /// By default, the name is derived from the implementing type.
        /// </summary>
        /// <example>
        /// The following query param service will match the query  displayed in URL
        /// `?include=some-relationship`
        /// <code>public class IncludeService : QueryParameterService  { /* ... */  } </code>
        /// </example>
        public virtual string Name { get { return GetParameterNameFromType(); } }

        /// <inheritdoc/>
        public abstract void Parse(string value);

        /// <summary>
        /// Gets the query parameter name from the implementing class name. Trims "Service"
        /// from the name if present.
        /// </summary>
        private string GetParameterNameFromType() => new Regex("Service$").Replace(GetType().Name, string.Empty);
    }
}
