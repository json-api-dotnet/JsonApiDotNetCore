using JsonApiDotNetCore.Internal.Contracts;
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Query;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <inheritdoc/>
    public class FieldsToSerialize : IFieldsToSerialize
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly ISparseFieldsService _sparseFieldsService ;
        private readonly IResourceDefinitionProvider _provider;

        public FieldsToSerialize(IResourceGraph resourceGraph,
                                 ISparseFieldsService sparseFieldsService,
                                 IResourceDefinitionProvider provider)
        {
            _resourceGraph = resourceGraph;
            _sparseFieldsService  = sparseFieldsService;
            _provider = provider;
        }

        /// <inheritdoc/>
        public List<AttrAttribute> GetAllowedAttributes(Type type, RelationshipAttribute relationship = null)
        {   // get the list of all exposed attributes for the given type.
            var allowed = _resourceGraph.GetAttributes(type);

            var resourceDefinition = _provider.Get(type);
            if (resourceDefinition != null)
                // The set of allowed attributes to be exposed was defined on the resource definition
                allowed = allowed.Intersect(resourceDefinition.GetAllowedAttributes()).ToList();

            var sparseFieldsSelection = _sparseFieldsService.Get(relationship);
            if (sparseFieldsSelection.Any())
                // from the allowed attributes, select the ones flagged by sparse field selection.
                allowed = allowed.Intersect(sparseFieldsSelection).ToList();

            return allowed;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Note: this method does NOT check if a relationship is included to determine
        /// if it should be serialized. This is because completely hiding a relationship
        /// is not the same as not including. In the case of the latter,
        /// we may still want to add the relationship to expose the navigation link to the client.
        /// </remarks>
        public List<RelationshipAttribute> GetAllowedRelationships(Type type)
        {
            var resourceDefinition = _provider.Get(type);
            if (resourceDefinition != null)
                // The set of allowed attributes to be exposed was defined on the resource definition
                return resourceDefinition.GetAllowedRelationships();

            // The set of allowed attributes to be exposed was NOT defined on the resource definition: return all
            return _resourceGraph.GetRelationships(type);
        }
    }
}
