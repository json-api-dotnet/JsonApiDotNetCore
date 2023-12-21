using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

public sealed class DivingBoardsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<DivingBoard, int> resourceService)
    : JsonApiController<DivingBoard, int>(options, resourceGraph, loggerFactory, resourceService);
