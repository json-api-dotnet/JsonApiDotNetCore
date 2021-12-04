using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// Represents a stripped-down copy of this type in the JsonApiDotNetCore project. It exists solely to fulfill the dependency needs for successfully
/// compiling the source-generated controllers in this project.
/// </summary>
[PublicAPI]
public abstract class JsonApiQueryController<TResource, TId> : BaseJsonApiController<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    protected JsonApiQueryController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceQueryService<TResource, TId> queryService)
        : base(options, resourceGraph, loggerFactory, queryService)
    {
    }
}
