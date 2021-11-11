using System;
using CosmosDbExample.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace CosmosDbExample.Controllers
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
