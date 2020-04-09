using System;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IResourceGraphBuilder
    {
        /// <summary>
        /// Construct the <see cref="ResourceGraph"/>
        /// </summary>
        IResourceGraph Build();
        /// <summary>
        /// Add a json:api resource
        /// </summary>
        /// <typeparam name="TResource">The resource model type</typeparam>
        /// <param name="pluralizedTypeName">
        /// The pluralized name that should be exposed by the API. 
        /// If nothing is specified, the configured name formatter will be used.
        /// </param>
        IResourceGraphBuilder AddResource<TResource>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<int>;
        /// <summary>
        /// Add a json:api resource
        /// </summary>
        /// <typeparam name="TResource">The resource model type</typeparam>
        /// <typeparam name="TId">The resource model identifier type</typeparam>
        /// <param name="pluralizedTypeName">
        /// The pluralized name that should be exposed by the API. 
        /// If nothing is specified, the configured name formatter will be used.
        /// </param>
        IResourceGraphBuilder AddResource<TResource, TId>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<TId>;
        /// <summary>
        /// Add a Json:Api resource
        /// </summary>
        /// <param name="entityType">The resource model type</param>
        /// <param name="idType">The resource model identifier type</param>
        /// <param name="pluralizedTypeName">
        /// The pluralized name that should be exposed by the API. 
        /// If nothing is specified, the configured name formatter will be used.
        /// </param>
        IResourceGraphBuilder AddResource(Type entityType, Type idType, string pluralizedTypeName = null);
    }
}
