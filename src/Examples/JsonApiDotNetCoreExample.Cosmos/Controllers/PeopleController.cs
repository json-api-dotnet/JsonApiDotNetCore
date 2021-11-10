using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Cosmos.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Cosmos.Controllers
{
    public sealed class PeopleController : JsonApiController<Person, Guid>
    {
        public PeopleController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Person, Guid> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
