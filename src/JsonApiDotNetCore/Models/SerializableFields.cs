using JsonApiDotNetCore.Internal.Contracts;
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Models
{
    public class SerializableFields : ISerializableFields
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IServiceProvider _provider;
        private readonly Dictionary<Type, IResourceDefinition> _resourceDefinitionCache = new Dictionary<Type, IResourceDefinition>();
        private readonly IExposedFieldExplorer _fieldExplorer;

        public SerializableFields(IExposedFieldExplorer fieldExplorer,
                                  IResourceGraph resourceGraph,
                                  IServiceProvider provider)
        {
            _fieldExplorer = fieldExplorer;
            _resourceGraph = resourceGraph;
            _provider = provider;
        }

        public List<AttrAttribute> GetAllowedAttributes(Type type)
        {
            var resourceDefinition = GetResourceDefinition(type);
            if (resourceDefinition != null)
                // The set of allowed attribrutes to be exposed was defined on the resource definition
                return resourceDefinition.GetAllowedAttributes();

            // The set of allowed attribrutes to be exposed was NOT defined on the resource definition: return all
            return _fieldExplorer.GetAttributes(type);
        }

        public List<RelationshipAttribute> GetAllowedRelationships(Type type)
        {
            var resourceDefinition = GetResourceDefinition(type);
            if (resourceDefinition != null)
                // The set of allowed attribrutes to be exposed was defined on the resource definition
                return resourceDefinition.GetAllowedRelationships();

            // The set of allowed attribrutes to be exposed was NOT defined on the resource definition: return all
            return _fieldExplorer.GetRelationships(type);
        }

        private IResourceDefinition GetResourceDefinition(Type resourceType)
        {

            var resourceDefinitionType = _resourceGraph.GetContextEntity(resourceType).ResourceType;
            if (!_resourceDefinitionCache.TryGetValue(resourceDefinitionType, out IResourceDefinition resourceDefinition))
            {
                resourceDefinition = _provider.GetService(resourceDefinitionType) as IResourceDefinition;
                _resourceDefinitionCache.Add(resourceDefinitionType, resourceDefinition);
            }
            return resourceDefinition;
        }
    }
}
