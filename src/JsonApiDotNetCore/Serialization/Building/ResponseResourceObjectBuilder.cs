using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    public class ResponseResourceObjectBuilder : ResourceObjectBuilder
    {
        private readonly IIncludedResourceObjectBuilder _includedBuilder;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly ILinkBuilder _linkBuilder;
        private readonly SparseFieldSetCache _sparseFieldSetCache;
        private RelationshipAttribute _requestRelationship;

        public ResponseResourceObjectBuilder(ILinkBuilder linkBuilder,
                                             IIncludedResourceObjectBuilder includedBuilder,
                                             IEnumerable<IQueryConstraintProvider> constraintProviders,
                                             IResourceContextProvider resourceContextProvider,
                                             IResourceDefinitionAccessor resourceDefinitionAccessor,
                                             IResourceObjectBuilderSettingsProvider settingsProvider)
            : base(resourceContextProvider, settingsProvider.Get())
        {
            _linkBuilder = linkBuilder ?? throw new ArgumentNullException(nameof(linkBuilder));
            _includedBuilder = includedBuilder ?? throw new ArgumentNullException(nameof(includedBuilder));
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));
            _resourceDefinitionAccessor = resourceDefinitionAccessor ?? throw new ArgumentNullException(nameof(resourceDefinitionAccessor));
            _sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
        }

        public RelationshipEntry Build(IIdentifiable resource, RelationshipAttribute requestRelationship)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            _requestRelationship = requestRelationship ?? throw new ArgumentNullException(nameof(requestRelationship));
            return GetRelationshipData(requestRelationship, resource);
        }

        /// <inheritdoc /> 
        public override ResourceObject Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes = null,
            IReadOnlyCollection<RelationshipAttribute> relationships = null)
        {
            var resourceObject = base.Build(resource, attributes, relationships);

            resourceObject.Meta = _resourceDefinitionAccessor.GetMeta(resource.GetType(), resource);

            return resourceObject;
        }

        /// <summary>
        /// Builds the values of the relationships object on a resource object.
        /// The server serializer only populates the "data" member when the relationship is included,
        /// and adds links unless these are turned off. This means that if a relationship is not included
        /// and links are turned off, the entry would be completely empty, ie { }, which is not conform
        /// JSON:API spec. In that case we return null which will omit the entry from the output.
        /// </summary>
        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            RelationshipEntry relationshipEntry = null;
            List<IReadOnlyCollection<RelationshipAttribute>> relationshipChains = null;
            if (Equals(relationship, _requestRelationship) || ShouldInclude(relationship, out relationshipChains))
            {
                relationshipEntry = base.GetRelationshipData(relationship, resource);
                if (relationshipChains != null && relationshipEntry.HasResource)
                    foreach (var chain in relationshipChains)
                        // traverses (recursively) and extracts all (nested) related resources for the current inclusion chain.
                        _includedBuilder.IncludeRelationshipChain(chain, resource);
            }

            if (!IsRelationshipInSparseFieldSet(relationship))
            {
                return null;
            }

            var links = _linkBuilder.GetRelationshipLinks(relationship, resource);
            if (links != null)
            {
                // if relationshipLinks should be built for this entry, populate the "links" field.
                relationshipEntry ??= new RelationshipEntry();
                relationshipEntry.Links = links;
            }

            // if neither "links" nor "data" was populated, return null, which will omit this entry from the output.
            // (see the NullValueHandling settings on <see cref="ResourceObject"/>)
            return relationshipEntry;
        }

        private bool IsRelationshipInSparseFieldSet(RelationshipAttribute relationship)
        {
            var resourceContext = ResourceContextProvider.GetResourceContext(relationship.LeftType);

            var fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceContext);
            return fieldSet.Contains(relationship);
        }

        /// <summary>
        /// Inspects the included relationship chains (see <see cref="IIncludeQueryStringParameterReader"/>
        /// to see if <paramref name="relationship"/> should be included or not.
        /// </summary>
        private bool ShouldInclude(RelationshipAttribute relationship, out List<IReadOnlyCollection<RelationshipAttribute>> inclusionChain)
        {
            var chains = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<IncludeExpression>()
                .SelectMany(IncludeChainConverter.GetRelationshipChains)
                .ToArray();

            inclusionChain = new List<IReadOnlyCollection<RelationshipAttribute>>();

            foreach (var chain in chains)
            {
                if (chain.Fields.First().Equals(relationship))
                {
                    inclusionChain.Add(chain.Fields.Cast<RelationshipAttribute>().ToArray());
                }
            }

            return inclusionChain.Any();
        }
    }
}
