using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Internal.QueryStrings;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Serialization.Server.Builders;

namespace JsonApiDotNetCore.Serialization.Server
{
    public class ResponseResourceObjectBuilder : ResourceObjectBuilder
    {
        private readonly IIncludedResourceObjectBuilder _includedBuilder;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly ILinkBuilder _linkBuilder;
        private RelationshipAttribute _requestRelationship;

        public ResponseResourceObjectBuilder(ILinkBuilder linkBuilder,
                                             IIncludedResourceObjectBuilder includedBuilder,
                                             IEnumerable<IQueryConstraintProvider> constraintProviders,
                                             IResourceContextProvider provider,
                                             IResourceObjectBuilderSettingsProvider settingsProvider)
            : base(provider, settingsProvider.Get())
        {
            _linkBuilder = linkBuilder;
            _includedBuilder = includedBuilder;
            _constraintProviders = constraintProviders;
        }

        public RelationshipEntry Build(IIdentifiable resource, RelationshipAttribute requestRelationship)
        {
            _requestRelationship = requestRelationship;
            return GetRelationshipData(requestRelationship, resource);
        }

        /// <summary>
        /// Builds the values of the relationships object on a resource object.
        /// The server serializer only populates the "data" member when the relationship is included,
        /// and adds links unless these are turned off. This means that if a relationship is not included
        /// and links are turned off, the entry would be completely empty, ie { }, which is not conform
        /// json:api spec. In that case we return null which will omit the entry from the output.
        /// </summary>
        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            RelationshipEntry relationshipEntry = null;
            List<List<RelationshipAttribute>> relationshipChains = null;
            if (Equals(relationship, _requestRelationship) || ShouldInclude(relationship, out relationshipChains))
            {
                relationshipEntry = base.GetRelationshipData(relationship, resource);
                if (relationshipChains != null && relationshipEntry.HasResource)
                    foreach (var chain in relationshipChains)
                        // traverses (recursively) and extracts all (nested) related resources for the current inclusion chain.
                        _includedBuilder.IncludeRelationshipChain(chain, resource);
            }

            var links = _linkBuilder.GetRelationshipLinks(relationship, resource);
            if (links != null)
                // if links relationshipLinks should be built for this entry, populate the "links" field.
                (relationshipEntry ??= new RelationshipEntry()).Links = links;

            // if neither "links" nor "data" was populated, return null, which will omit this entry from the output.
            // (see the NullValueHandling settings on <see cref="ResourceObject"/>)
            return relationshipEntry;
        }

        /// <summary>
        /// Inspects the included relationship chains (see <see cref="IIncludeQueryStringParameterReader"/>
        /// to see if <paramref name="relationship"/> should be included or not.
        /// </summary>
        private bool ShouldInclude(RelationshipAttribute relationship, out List<List<RelationshipAttribute>> inclusionChain)
        {
            var includes = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<IncludeExpression>()
                .ToArray();

            inclusionChain = new List<List<RelationshipAttribute>>();

            foreach (var chain in includes.SelectMany(IncludeChainConverter.GetRelationshipChains))
            {
                if (chain.Fields.First().Equals(relationship))
                {
                    inclusionChain.Add(chain.Fields.Cast<RelationshipAttribute>().ToList());
                }
            }

            return inclusionChain.Any();
        }
    }
}
