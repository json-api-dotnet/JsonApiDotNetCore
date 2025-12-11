using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

public sealed class DivingBoardsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<DivingBoard, long> resourceService)
    : JsonApiController<DivingBoard, long>(options, resourceGraph, loggerFactory, resourceService);
