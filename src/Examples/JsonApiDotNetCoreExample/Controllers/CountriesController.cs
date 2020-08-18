using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableQueryString(StandardQueryStringParameters.Sort | StandardQueryStringParameters.Page)]
    public sealed class CountriesController : JsonApiController<Country>
    {
        public CountriesController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Country> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
