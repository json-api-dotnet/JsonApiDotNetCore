using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter
// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Represents a stripped-down copy of this type in the JsonApiDotNetCore project. It exists solely to fulfill the dependency needs for successfully
    /// compiling the source-generated controllers in this project.
    /// </summary>
    [PublicAPI]
    public interface IGetRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
    }
}
