using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<Person> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
