using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Represents a stripped-down copy of this type in the JsonApiDotNetCore project. It exists solely to fulfill the dependency needs for successfully
    /// compiling the source-generated controllers in this project.
    /// </summary>
    [PublicAPI]
    public abstract class BaseJsonApiController<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        protected BaseJsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<TResource, TId> resourceService)
            : this(options, resourceGraph, loggerFactory, resourceService, resourceService)
        {
        }

        protected BaseJsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, TId>? queryService = null, IResourceCommandService<TResource, TId>? commandService = null)
            : this(options, resourceGraph, loggerFactory, queryService, queryService, queryService, queryService, commandService, commandService,
                commandService, commandService, commandService, commandService)
        {
        }

        protected BaseJsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IGetAllService<TResource, TId>? getAll = null, IGetByIdService<TResource, TId>? getById = null,
            IGetSecondaryService<TResource, TId>? getSecondary = null, IGetRelationshipService<TResource, TId>? getRelationship = null,
            ICreateService<TResource, TId>? create = null, IAddToRelationshipService<TResource, TId>? addToRelationship = null,
            IUpdateService<TResource, TId>? update = null, ISetRelationshipService<TResource, TId>? setRelationship = null,
            IDeleteService<TResource, TId>? delete = null, IRemoveFromRelationshipService<TResource, TId>? removeFromRelationship = null)
        {
        }
    }
}
