using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    public class FieldsToSerialize : IFieldsToSerialize
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IJsonApiRequest _request;
        private readonly SparseFieldSetCache _sparseFieldSetCache;

        public FieldsToSerialize(
            IResourceContextProvider resourceContextProvider,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiRequest request)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            if (_request.Kind == EndpointKind.Relationship)
            {
                return Array.Empty<AttrAttribute>();
            }

            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            var fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceContext);

            return fieldSet.OfType<AttrAttribute>().ToArray();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Note: this method does NOT check if a relationship is included to determine
        /// if it should be serialized. This is because completely hiding a relationship
        /// is not the same as not including. In the case of the latter,
        /// we may still want to add the relationship to expose the navigation link to the client.
        /// </remarks>
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            if (_request.Kind == EndpointKind.Relationship)
            {
                return Array.Empty<RelationshipAttribute>();
            }

            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            return resourceContext.Relationships;
        }

        /// <inheritdoc />
        public void ResetCache()
        {
            _sparseFieldSetCache.Reset();
        }
    }
}
