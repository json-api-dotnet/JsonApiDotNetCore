using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization.Server.Builders;

namespace JsonApiDotNetCore.Serialization.Server
{
    public class ResponseResourceObjectBuilder : ResourceObjectBuilder, IResourceObjectBuilder
    {
        private readonly IIncludedResourceObjectBuilder _includedBuilder;
        private readonly IIncludeService _includeService;
        private readonly ILinkBuilder _linkBuilder;
        private RelationshipAttribute _requestRelationship;

        public ResponseResourceObjectBuilder(ILinkBuilder linkBuilder,
                                             IIncludedResourceObjectBuilder includedBuilder,
                                             IIncludeService includeService,
                                             IResourceContextProvider provider,
                                             IResourceObjectBuilderSettingsProvider settingsProvider)
            : base(provider, settingsProvider.Get())
        {
            _linkBuilder = linkBuilder;
            _includedBuilder = includedBuilder;
            _includeService = includeService;
        }

        public RelationshipEntry Build(IIdentifiable entity, RelationshipAttribute requestRelationship)
        {
            _requestRelationship = requestRelationship;
            return GetRelationshipData(requestRelationship, entity);
        }

        /// <summary>
        /// Builds the values of the relationships object on a resource object.
        /// The server serializer only populates the "data" member when the relationship is included,
        /// and adds links unless these are turned off. This means that if a relationship is not included
        /// and links are turned off, the entry would be completely empty, ie { }, which is not conform
        /// json:api spec. In that case we return null which will omit the entry from the output.
        /// </summary>
        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            RelationshipEntry relationshipEntry = null;
            List<List<RelationshipAttribute>> relationshipChains = null;
            if (relationship == _requestRelationship || ShouldInclude(relationship, out relationshipChains ))
            {
                relationshipEntry = base.GetRelationshipData(relationship, entity);
                if (relationshipChains != null && relationshipEntry.HasResource)
                    foreach (var chain in relationshipChains)
                        // traverses (recursively) and extracts all (nested) related entities for the current inclusion chain.
                        _includedBuilder.IncludeRelationshipChain(chain, entity);
            }

            var links = _linkBuilder.GetRelationshipLinks(relationship, entity);
            if (links != null)
                // if links relationshipLinks should be built for this entry, populate the "links" field.
                (relationshipEntry = relationshipEntry ?? new RelationshipEntry()).Links = links;

            /// if neither "links" nor "data" was popupated, return null, which will omit this entry from the output.
            /// (see the NullValueHandling settings on <see cref="ResourceObject"/>)
            return relationshipEntry;
        }

        /// <summary>
        /// Inspects the included relationship chains (see <see cref="IIncludeService"/>
        /// to see if <paramref name="relationship"/> should be included or not.
        /// </summary>
        private bool ShouldInclude(RelationshipAttribute relationship, out List<List<RelationshipAttribute>> inclusionChain)
        {
            inclusionChain = _includeService.Get()?.Where(l => l.First().Equals(relationship)).ToList();
            if (inclusionChain == null || !inclusionChain.Any())
                return false;
            return true;
        }
    }
}
