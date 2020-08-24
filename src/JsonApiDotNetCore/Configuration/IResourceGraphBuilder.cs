using System;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    public interface IResourceGraphBuilder
    {
        /// <summary>
        /// Constructs the <see cref="IResourceGraph"/>.
        /// </summary>
        IResourceGraph Build();
        /// <summary>
        /// Adds a json:api resource.
        /// </summary>
        /// <typeparam name="TResource">The resource model type.</typeparam>
        /// <param name="pluralizedTypeName">
        /// The pluralized name, under which the resource is publicly exposed by the API. 
        /// If nothing is specified, the configured casing convention formatter will be applied.
        /// </param>
        IResourceGraphBuilder Add<TResource>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<int>;
        /// <summary>
        /// Adds a json:api resource.
        /// </summary>
        /// <typeparam name="TResource">The resource model type.</typeparam>
        /// <typeparam name="TId">The resource model identifier type.</typeparam>
        /// <param name="pluralizedTypeName">
        /// The pluralized name, under which the resource is publicly exposed by the API. 
        /// If nothing is specified, the configured casing convention formatter will be applied.
        /// </param>
        IResourceGraphBuilder Add<TResource, TId>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<TId>;
        /// <summary>
        /// Adds a json:api resource.
        /// </summary>
        /// <param name="resourceType">The resource model type.</param>
        /// <param name="idType">The resource model identifier type.</param>
        /// <param name="pluralizedTypeName">
        /// The pluralized name, under which the resource is publicly exposed by the API. 
        /// If nothing is specified, the configured casing convention formatter will be applied.
        /// </param>
        IResourceGraphBuilder Add(Type resourceType, Type idType = null, string pluralizedTypeName = null);
    }
}
