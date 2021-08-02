using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    [PublicAPI]
    public class FieldsToSerialize : IFieldsToSerialize
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IJsonApiRequest _request;
        private readonly SparseFieldSetCache _sparseFieldSetCache;

        /// <inheritdoc />
        public bool ShouldSerialize => _request.Kind != EndpointKind.Relationship;

        public FieldsToSerialize(IResourceContextProvider resourceContextProvider, IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceDefinitionAccessor resourceDefinitionAccessor, IJsonApiRequest request)
        {
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(request, nameof(request));

            _resourceContextProvider = resourceContextProvider;
            _request = request;
            _sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (!ShouldSerialize)
            {
                return Array.Empty<AttrAttribute>();
            }

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceContext);

            return SortAttributesInDeclarationOrder(fieldSet, resourceContext).ToArray();
        }

        private IEnumerable<AttrAttribute> SortAttributesInDeclarationOrder(IImmutableSet<ResourceFieldAttribute> fieldSet, ResourceContext resourceContext)
        {
            foreach (AttrAttribute attribute in resourceContext.Attributes)
            {
                if (fieldSet.Contains(attribute))
                {
                    yield return attribute;
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Note: this method does NOT check if a relationship is included to determine if it should be serialized. This is because completely hiding a
        /// relationship is not the same as not including. In the case of the latter, we may still want to add the relationship to expose the navigation link to
        /// the client.
        /// </remarks>
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (!ShouldSerialize)
            {
                return Array.Empty<RelationshipAttribute>();
            }

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            return resourceContext.Relationships;
        }

        /// <inheritdoc />
        public void ResetCache()
        {
            _sparseFieldSetCache.Reset();
        }
    }
}
