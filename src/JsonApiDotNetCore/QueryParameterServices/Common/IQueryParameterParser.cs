using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Responsible for populating the various service implementations of
    /// <see cref="IQueryParameterService"/>.
    /// </summary>
    public interface IQueryParameterParser
    {
        void Parse(DisableQueryAttribute disabledQuery = null);
    }
}
