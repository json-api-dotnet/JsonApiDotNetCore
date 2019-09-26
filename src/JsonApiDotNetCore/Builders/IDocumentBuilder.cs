using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IDocumentBuilder
    {
        /// <summary>
        /// Builds a Json:Api document from the provided resource instance.
        /// </summary>
        /// <param name="entity">The resource to convert.</param>
        Document Build(IIdentifiable entity);

        /// <summary>
        /// Builds a json:api document from the provided resource instances.
        /// </summary>
        /// <param name="entities">The collection of resources to convert.</param>
        //Documents Build(IEnumerable<IIdentifiable> entities);

        [Obsolete("You should specify an IResourceDefinition implementation using the GetData/3 overload.")]
        ResourceObject GetData(ContextEntity contextEntity, IIdentifiable entity);

        /// <summary>
        /// Create the resource object for the provided resource.
        /// </summary>
        /// <param name="contextEntity">The metadata for the resource.</param>
        /// <param name="entity">The resource instance.</param>
        /// <param name="resourceDefinition">
        /// The resource definition (optional). This can be used for filtering out attributes
        /// that should not be exposed to the client. For example, you might want to limit
        /// the exposed attributes based on the authenticated user's role.
        /// </param>
        ResourceObject GetData(ContextEntity contextEntity, IIdentifiable entity,  object resourceDefinition = null);
    }
}
