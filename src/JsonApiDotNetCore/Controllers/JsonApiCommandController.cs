using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// The base class to derive resource-specific write-only controllers from. Returns HTTP 405 on read-only endpoints. If you want to provide routing
/// templates yourself, you should derive from BaseJsonApiController directly.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
public abstract class JsonApiCommandController<TResource, TId>(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceCommandService<TResource, TId> commandService)
    : JsonApiController<TResource, TId>(options, resourceGraph, loggerFactory, null, null, null, null, commandService, commandService, commandService,
        commandService, commandService, commandService)
    where TResource : class, IIdentifiable<TId>;
