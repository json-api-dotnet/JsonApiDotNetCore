using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

public sealed class DuplicateKnownResourcesController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<KnownResource, int> resourceService)
    : JsonApiController<KnownResource, int>(options, resourceGraph, loggerFactory, resourceService);
