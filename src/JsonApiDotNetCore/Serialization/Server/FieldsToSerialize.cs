using JsonApiDotNetCore.Internal.Contracts;
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Query;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <inheritdoc/>
    /// TODO: explore option out caching so we don't have to recalculate the list
    /// of allowed attributes and relationships all the time. This is more efficient
    /// for documents with many resource objects.
    public class FieldsToSerialize : IFieldsToSerialize
    {
        private readonly IContextEntityProvider _resourceContextProvider;
        private readonly ISparseFieldsService _sparseFieldsService ;
        private readonly IServiceProvider _provider;
        private readonly Dictionary<Type, IResourceDefinition> _resourceDefinitionCache = new Dictionary<Type, IResourceDefinition>();
        private readonly IFieldsExplorer _fieldExplorer;

        public FieldsToSerialize(IFieldsExplorer fieldExplorer,
                                 IContextEntityProvider resourceContextProvider,
                                 ISparseFieldsService sparseFieldsService,
                                 IServiceProvider provider)
        {
            _fieldExplorer = fieldExplorer;
            _resourceContextProvider = resourceContextProvider;
            _sparseFieldsService  = sparseFieldsService;
            _provider = provider;
        }

        /// <inheritdoc/>
        public List<AttrAttribute> GetAllowedAttributes(Type type, RelationshipAttribute relationship = null)
        {   // get the list of all exposed atttributes for the given type.
            var allowed = _fieldExplorer.GetAttributes(type);

            var resourceDefinition = GetResourceDefinition(type);
            if (resourceDefinition != null)
                // The set of allowed attribrutes to be exposed was defined on the resource definition
                allowed = allowed.Intersect(resourceDefinition.GetAllowedAttributes()).ToList();

            var sparseFieldsSelection = _sparseFieldsService.Get(relationship);
            if (sparseFieldsSelection != null && sparseFieldsSelection.Any())
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
            var resourceDefinition = GetResourceDefinition(type);
            if (resourceDefinition != null)
                // The set of allowed attribrutes to be exposed was defined on the resource definition
                return resourceDefinition.GetAllowedRelationships();

            // The set of allowed attribrutes to be exposed was NOT defined on the resource definition: return all
            return _fieldExplorer.GetRelationships(type);
        }


        /// consider to implement and inject a `ResourceDefinitionProvider` service.
        private IResourceDefinition GetResourceDefinition(Type resourceType)
        {

            var resourceDefinitionType = _resourceContextProvider.GetContextEntity(resourceType).ResourceType;
            if (!_resourceDefinitionCache.TryGetValue(resourceDefinitionType, out IResourceDefinition resourceDefinition))
            {
                resourceDefinition = _provider.GetService(resourceDefinitionType) as IResourceDefinition;
                _resourceDefinitionCache.Add(resourceDefinitionType, resourceDefinition);
            }
            return resourceDefinition;
        }
    }
}
