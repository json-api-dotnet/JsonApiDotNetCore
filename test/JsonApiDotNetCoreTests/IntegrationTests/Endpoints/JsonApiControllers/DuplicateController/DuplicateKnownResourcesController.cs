using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.DuplicateController;

public sealed class DuplicateKnownResourcesController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<KnownResource, long> resourceService)
    : JsonApiController<KnownResource, long>(options, resourceGraph, loggerFactory, resourceService);
