using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    [PublicAPI]
    public class ResponseResourceObjectBuilder : ResourceObjectBuilder
    {
        private static readonly IncludeChainConverter IncludeChainConverter = new();

        private readonly ILinkBuilder _linkBuilder;
        private readonly IIncludedResourceObjectBuilder _includedBuilder;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
        private readonly SparseFieldSetCache _sparseFieldSetCache;

        private RelationshipAttribute _requestRelationship;

        public ResponseResourceObjectBuilder(ILinkBuilder linkBuilder, IIncludedResourceObjectBuilder includedBuilder,
            IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceGraph resourceGraph, IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiOptions options, IEvaluatedIncludeCache evaluatedIncludeCache)
            : base(resourceGraph, options)
        {
            ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
            ArgumentGuard.NotNull(includedBuilder, nameof(includedBuilder));
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(evaluatedIncludeCache, nameof(evaluatedIncludeCache));

            _linkBuilder = linkBuilder;
            _includedBuilder = includedBuilder;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _evaluatedIncludeCache = evaluatedIncludeCache;
            _sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
        }

        public RelationshipObject Build(IIdentifiable resource, RelationshipAttribute requestRelationship)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));
            ArgumentGuard.NotNull(requestRelationship, nameof(requestRelationship));

            _requestRelationship = requestRelationship;
            return GetRelationshipData(requestRelationship, resource);
        }

        /// <inheritdoc />
        public override ResourceObject Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes = null,
            IReadOnlyCollection<RelationshipAttribute> relationships = null)
        {
            ResourceObject resourceObject = base.Build(resource, attributes, relationships);

            resourceObject.Meta = _resourceDefinitionAccessor.GetMeta(resource.GetType(), resource);

            return resourceObject;
        }

        /// <summary>
        /// Builds a <see cref="RelationshipObject" /> for the specified relationship on a resource. The serializer only populates the "data" member when the
        /// relationship is included, and adds links unless these are turned off. This means that if a relationship is not included and links are turned off, the
        /// object would be completely empty, ie { }, which is not conform JSON:API spec. In that case we return null, which will omit the object from the
        /// output.
        /// </summary>
        protected override RelationshipObject GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(resource, nameof(resource));

            RelationshipObject relationshipObject = null;
            IReadOnlyCollection<IReadOnlyCollection<RelationshipAttribute>> relationshipChains = GetInclusionChainsStartingWith(relationship);

            if (Equals(relationship, _requestRelationship) || relationshipChains.Any())
            {
                relationshipObject = base.GetRelationshipData(relationship, resource);

                if (relationshipChains.Any() && relationshipObject.Data.Value != null)
                {
                    foreach (IReadOnlyCollection<RelationshipAttribute> chain in relationshipChains)
                    {
                        // traverses (recursively) and extracts all (nested) related resources for the current inclusion chain.
                        _includedBuilder.IncludeRelationshipChain(chain, resource);
                    }
                }
            }

            if (!IsRelationshipInSparseFieldSet(relationship))
            {
                return null;
            }

            RelationshipLinks links = _linkBuilder.GetRelationshipLinks(relationship, resource);

            if (links != null)
            {
                // if relationshipLinks should be built, populate the "links" field.
                relationshipObject ??= new RelationshipObject();
                relationshipObject.Links = links;
            }

            // if neither "links" nor "data" was populated, return null, which will omit this object from the output.
            // (see the NullValueHandling settings on <see cref="ResourceObject"/>)
            return relationshipObject;
        }

        private bool IsRelationshipInSparseFieldSet(RelationshipAttribute relationship)
        {
            ResourceContext resourceContext = ResourceGraph.GetResourceContext(relationship.LeftType);

            IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceContext);
            return fieldSet.Contains(relationship);
        }

        /// <summary>
        /// Inspects the included relationship chains and selects the ones that starts with the specified relationship.
        /// </summary>
        private IReadOnlyCollection<IReadOnlyCollection<RelationshipAttribute>> GetInclusionChainsStartingWith(RelationshipAttribute relationship)
        {
            IncludeExpression include = _evaluatedIncludeCache.Get() ?? IncludeExpression.Empty;
            IReadOnlyCollection<ResourceFieldChainExpression> chains = IncludeChainConverter.GetRelationshipChains(include);

            var inclusionChains = new List<IReadOnlyCollection<RelationshipAttribute>>();

            foreach (ResourceFieldChainExpression chain in chains)
            {
                if (chain.Fields[0].Equals(relationship))
                {
                    inclusionChains.Add(chain.Fields.Cast<RelationshipAttribute>().ToArray());
                }
            }

            return inclusionChains;
        }
    }
}
