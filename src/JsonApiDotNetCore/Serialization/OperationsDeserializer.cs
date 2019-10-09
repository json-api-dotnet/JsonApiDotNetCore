using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Serialization.Deserializer
{
    /// <summary>
    /// Legacy document parser to be used for Bulk requests.
    /// Will probably remove this for v4.
    /// </summary>
    public class OperationsDeserializer : IOperationsDeserializer
    {
        private readonly ITargetedFields _targetedFieldsManager;
        private readonly IResourceGraph _resourceGraph;
        private readonly JsonSerializer _jsonSerializer;

        public OperationsDeserializer(ITargetedFields updatedFieldsManager,
                                      IResourceGraph resourceGraph)
        {
            _targetedFieldsManager = updatedFieldsManager;
            _resourceGraph = resourceGraph;
        }

        public object Deserialize(string requestBody)
        {
            try
            {
                JToken bodyJToken;
                using (JsonReader jsonReader = new JsonTextReader(new StringReader(requestBody)))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    bodyJToken = JToken.Load(jsonReader);
                }
                var operations = JsonConvert.DeserializeObject<OperationsDocument>(requestBody);
                if (operations == null)
                    throw new JsonApiException(400, "Failed to deserialize operations request.");

                return operations;
            }
            catch (JsonApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new JsonApiException(400, "Failed to deserialize request body", e);
            }
        }

        public object DocumentToObject(ResourceObject data, List<ResourceObject> included = null)
        {
            if (data == null)
                throw new JsonApiException(422, "Failed to deserialize document as json:api.");

            var contextEntity = _resourceGraph.GetContextEntity(data.Type?.ToString());
            if (contextEntity == null)
            {
                throw new JsonApiException(400,
                    message: $"This API does not contain a json:api resource named '{data.Type}'.",
                    detail: "This resource is not registered on the ResourceGraph. "
                            + "If you are using Entity Framework, make sure the DbSet matches the expected resource name. "
                            + "If you have manually registered the resource, check that the call to AddResource correctly sets the public name.");
            }


            var entity = Activator.CreateInstance(contextEntity.EntityType);

            entity = SetEntityAttributes(entity, contextEntity, data.Attributes);
            entity = SetRelationships(entity, contextEntity, data.Relationships, included);

            var identifiableEntity = (IIdentifiable)entity;

            if (data.Id != null)
                identifiableEntity.StringId = data.Id?.ToString();

            return identifiableEntity;
        }

        private object SetEntityAttributes(
            object entity, ContextEntity contextEntity, Dictionary<string, object> attributeValues)
        {
            if (attributeValues == null || attributeValues.Count == 0)
                return entity;

            foreach (var attr in contextEntity.Attributes)
            {
                if (attributeValues.TryGetValue(attr.PublicAttributeName, out object newValue))
                {
                    if (attr.IsImmutable)
                        continue;
                    var convertedValue = ConvertAttrValue(newValue, attr.PropertyInfo.PropertyType);
                    attr.SetValue(entity, convertedValue);
                    _targetedFieldsManager.Attributes.Add(attr);
                }
            }

            return entity;
        }

        private object ConvertAttrValue(object newValue, Type targetType)
        {
            if (newValue is JContainer jObject)
                return DeserializeComplexType(jObject, targetType);

            var convertedValue = TypeHelper.ConvertType(newValue, targetType);
            return convertedValue;
        }

        private object DeserializeComplexType(JContainer obj, Type targetType)
        {
            return obj.ToObject(targetType, _jsonSerializer);
        }

        private object SetRelationships(
            object entity,
            ContextEntity contextEntity,
            Dictionary<string, RelationshipEntry> relationships,
            List<ResourceObject> included = null)
        {
            if (relationships == null || relationships.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();

            foreach (var attr in contextEntity.Relationships)
            {
                entity = attr.IsHasOne
                    ? SetHasOneRelationship(entity, entityProperties, (HasOneAttribute)attr, contextEntity, relationships, included)
                    : SetHasManyRelationship(entity, entityProperties, (HasManyAttribute)attr, contextEntity, relationships, included);
            }

            return entity;
        }

        private object SetHasOneRelationship(object entity,
            PropertyInfo[] entityProperties,
            HasOneAttribute attr,
            ContextEntity contextEntity,
            Dictionary<string, RelationshipEntry> relationships,
            List<ResourceObject> included = null)
        {
            var relationshipName = attr.PublicRelationshipName;

            if (relationships.TryGetValue(relationshipName, out RelationshipEntry relationshipData) == false)
                return entity;

            var rio = (ResourceIdentifierObject)relationshipData.Data;

            var foreignKey = attr.IdentifiablePropertyName;
            var foreignKeyProperty = entityProperties.FirstOrDefault(p => p.Name == foreignKey);
            if (foreignKeyProperty == null && rio == null)
                return entity;

            SetHasOneForeignKeyValue(entity, attr, foreignKeyProperty, rio);
            SetHasOneNavigationPropertyValue(entity, attr, rio, included);

            // recursive call ...
            if (included != null)
            {
                var navigationPropertyValue = attr.GetValue(entity);

                var resourceGraphEntity = _resourceGraph.GetContextEntity(attr.DependentType);
                if (navigationPropertyValue != null && resourceGraphEntity != null)

                {
                    var includedResource = included.SingleOrDefault(r => r.Type == rio.Type && r.Id == rio.Id);
                    if (includedResource != null)
                        SetRelationships(navigationPropertyValue, resourceGraphEntity, includedResource.Relationships, included);
                }
            }

            return entity;
        }

        private void SetHasOneForeignKeyValue(object entity, HasOneAttribute hasOneAttr, PropertyInfo foreignKeyProperty, ResourceIdentifierObject rio)
        {
            var foreignKeyPropertyValue = rio?.Id ?? null;
            if (foreignKeyProperty != null)
            {
                // in the case of the HasOne independent side of the relationship, we should still create the shell entity on the other side
                // we should not actually require the resource to have a foreign key (be the dependent side of the relationship)

                // e.g. PATCH /articles
                // {... { "relationships":{ "Owner": { "data": null } } } }
                bool foreignKeyPropertyIsNullableType = Nullable.GetUnderlyingType(foreignKeyProperty.PropertyType) != null
                    || foreignKeyProperty.PropertyType == typeof(string);
                if (rio == null && !foreignKeyPropertyIsNullableType)
                    throw new JsonApiException(400, $"Cannot set required relationship identifier '{hasOneAttr.IdentifiablePropertyName}' to null because it is a non-nullable type.");

                var convertedValue = TypeHelper.ConvertType(foreignKeyPropertyValue, foreignKeyProperty.PropertyType);
                /// todo: as a part of the process of decoupling JADNC (specifically 
                /// through the decoupling IJsonApiContext), we now no longer need to 
                /// store the updated relationship values in this property. For now 
                /// just assigning null as value, will remove this property later as a whole.
                /// see #512
                if (convertedValue == null) _targetedFieldsManager.Relationships.Add(hasOneAttr);
            }
        }

        /// <summary>
        /// Sets the value of the navigation property for the related resource.
        /// If the resource has been included, all attributes will be set.
        /// If the resource has not been included, only the id will be set.
        /// </summary>
        private void SetHasOneNavigationPropertyValue(object entity, HasOneAttribute hasOneAttr, ResourceIdentifierObject rio, List<ResourceObject> included)
        {
            // if the resource identifier is null, there should be no reason to instantiate an instance
            if (rio != null && rio.Id != null)
            {
                // we have now set the FK property on the resource, now we need to check to see if the
                // related entity was included in the payload and update its attributes
                var includedRelationshipObject = GetIncludedRelationship(rio, included, hasOneAttr);
                if (includedRelationshipObject != null)
                    hasOneAttr.SetValue(entity, includedRelationshipObject);

                /// todo: as a part of the process of decoupling JADNC (specifically 
                /// through the decoupling IJsonApiContext), we now no longer need to 
                /// store the updated relationship values in this property. For now 
                /// just assigning null as value, will remove this property later as a whole.
                /// see #512
                _targetedFieldsManager.Relationships.Add(hasOneAttr);
            }
        }

        private object SetHasManyRelationship(object entity,
            PropertyInfo[] entityProperties,
            HasManyAttribute attr,
            ContextEntity contextEntity,
            Dictionary<string, RelationshipEntry> relationships,
            List<ResourceObject> included = null)
        {
            var relationshipName = attr.PublicRelationshipName;

            if (relationships.TryGetValue(relationshipName, out RelationshipEntry relationshipData))
            {
                if (relationshipData.IsManyData == false)
                    return entity;

                var relatedResources = relationshipData.ManyData.Select(r =>
                {
                    var instance = GetIncludedRelationship(r, included, attr);
                    return instance;
                });

                var convertedCollection = TypeHelper.ConvertCollection(relatedResources, attr.DependentType);

                attr.SetValue(entity, convertedCollection);
                _targetedFieldsManager.Relationships.Add(attr);
            }

            return entity;
        }

        private IIdentifiable GetIncludedRelationship(ResourceIdentifierObject relatedResourceIdentifier, List<ResourceObject> includedResources, RelationshipAttribute relationshipAttr)
        {
            // at this point we can be sure the relationshipAttr.Type is IIdentifiable because we were able to successfully build the ResourceGraph
            var relatedInstance = relationshipAttr.DependentType.New<IIdentifiable>();
            relatedInstance.StringId = relatedResourceIdentifier.Id;

            // can't provide any more data other than the rio since it is not contained in the included section
            if (includedResources == null || includedResources.Count == 0)
                return relatedInstance;

            var includedResource = GetLinkedResource(relatedResourceIdentifier, includedResources);
            if (includedResource == null)
                return relatedInstance;

            var contextEntity = _resourceGraph.GetContextEntity(relationshipAttr.DependentType);
            if (contextEntity == null)
                throw new JsonApiException(400, $"Included type '{relationshipAttr.DependentType}' is not a registered json:api resource.");

            SetEntityAttributes(relatedInstance, contextEntity, includedResource.Attributes);

            return relatedInstance;
        }

        private ResourceObject GetLinkedResource(ResourceIdentifierObject relatedResourceIdentifier, List<ResourceObject> includedResources)
        {
            try
            {
                return includedResources.SingleOrDefault(r => r.Type == relatedResourceIdentifier.Type && r.Id == relatedResourceIdentifier.Id);
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"A compound document MUST NOT include more than one resource object for each type and id pair."
                        + $"The duplicate pair was '{relatedResourceIdentifier.Type}, {relatedResourceIdentifier.Id}'", e);
            }
        }
    }
}