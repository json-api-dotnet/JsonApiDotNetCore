using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Represents a stripped-down copy of this type in the JsonApiDotNetCore project. It exists solely to fulfill the dependency needs for successfully
    /// compiling the source-generated controllers in this project.
    /// </summary>
    [PublicAPI]
    public interface IResourceQueryService<TResource, in TId>
        : IGetAllService<TResource, TId>, IGetByIdService<TResource, TId>, IGetRelationshipService<TResource, TId>, IGetSecondaryService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
    }
}
