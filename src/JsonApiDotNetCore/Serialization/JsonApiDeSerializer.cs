using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Serialization
{
    public class JsonApiDeSerializer : IJsonApiDeSerializer
    {
        private readonly IJsonApiContext _jsonApiContext;

        [Obsolete(
            "The deserializer no longer depends on the IGenericProcessorFactory",
            error: false)]
        public JsonApiDeSerializer(
            IJsonApiContext jsonApiContext,
            IGenericProcessorFactory genericProcessorFactory)
        {
            _jsonApiContext = jsonApiContext;
        }

        public JsonApiDeSerializer(IJsonApiContext jsonApiContext)
        {
            _jsonApiContext = jsonApiContext;
        }

        public object Deserialize(string requestBody)
        {
            try
            {
                var bodyJToken = JToken.Parse(requestBody);

                if (RequestIsOperation(bodyJToken))
                {
                    _jsonApiContext.IsBulkOperationRequest = true;

                    // TODO: determine whether or not the token should be re-used rather than performing full
                    // deserialization again from the string
                    var operations = JsonConvert.DeserializeObject<OperationsDocument>(requestBody);
                    if (operations == null)
                        throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = "Failed to deserialize operations request."
                    });

                    return operations;
                }

                var document = bodyJToken.ToObject<Document>();

                _jsonApiContext.DocumentMeta = document.Meta;
                var entity = DocumentToObject(document.Data, document.Included);
                return entity;
            }
            catch (JsonApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Failed to deserialize request body",
                    Source = e
                });
            }
        }

        private bool RequestIsOperation(JToken bodyJToken)
            => _jsonApiContext.Options.EnableOperations
                && (bodyJToken.SelectToken("operations") != null);

        public TEntity Deserialize<TEntity>(string requestBody) => (TEntity)Deserialize(requestBody);

        public object DeserializeRelationship(string requestBody)
        {
            try
            {
                var data = JToken.Parse(requestBody)["data"];

                if (data is JArray)
                    return data.ToObject<List<ResourceObject>>();

                return new List<ResourceObject> { data.ToObject<ResourceObject>() };
            }
            catch (Exception e)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Failed to deserialize request body",
                    Source = e
                });
            }
        }

        public List<TEntity> DeserializeList<TEntity>(string requestBody)
        {
            try
            {
                var documents = JsonConvert.DeserializeObject<Documents>(requestBody);

                var deserializedList = new List<TEntity>();
                foreach (var data in documents.Data)
                {
                    var entity = (TEntity)DocumentToObject(data, documents.Included);
                    deserializedList.Add(entity);
                }

                return deserializedList;
            }
            catch (Exception e)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Failed to deserialize request body",
                    Source = e
                });
            }
        }

        public object DocumentToObject(ResourceObject data, List<ResourceObject> included = null)
        {
            if (data == null)
                throw new JsonApiException(new Error(HttpStatusCode.UnprocessableEntity)
                    {
                        Title = "Failed to deserialize document as json:api."
                    });

            var contextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(data.Type?.ToString());
            _jsonApiContext.RequestEntity = contextEntity ?? throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
            {
                Title =  $"This API does not contain a json:api resource named '{data.Type}'.",
                Detail =  "This resource is not registered on the ResourceGraph. "
                        + "If you are using Entity Framework, make sure the DbSet matches the expected resource name. "
                        + "If you have manually registered the resource, check that the call to AddResource correctly sets the public name."
            });

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
                    var convertedValue = ConvertAttrValue(newValue, attr.PropertyInfo.PropertyType);
                    attr.SetValue(entity, convertedValue);

                    if (attr.IsImmutable == false)
                        _jsonApiContext.AttributesToUpdate[attr] = convertedValue;
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
            return obj.ToObject(targetType, JsonSerializer.Create(_jsonApiContext.Options.SerializerSettings));
        }

        private object SetRelationships(
            object entity,
            ContextEntity contextEntity,
            Dictionary<string, RelationshipData> relationships,
            List<ResourceObject> included = null)
        {
            if (relationships == null || relationships.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();

            foreach (var attr in contextEntity.Relationships)
            {
                entity = attr.IsHasOne
                    ? SetHasOneRelationship(entity, entityProperties, (HasOneAttribute)attr, contextEntity, relationships, included)
                    : SetHasManyRelationship(entity, entityProperties, attr, contextEntity, relationships, included);
            }

            return entity;
        }

        private object SetHasOneRelationship(object entity,
            PropertyInfo[] entityProperties,
            HasOneAttribute attr,
            ContextEntity contextEntity,
            Dictionary<string, RelationshipData> relationships,
            List<ResourceObject> included = null)
        {
            var relationshipName = attr.PublicRelationshipName;

            if (relationships.TryGetValue(relationshipName, out RelationshipData relationshipData) == false)
                return entity;

            var rio = (ResourceIdentifierObject)relationshipData.ExposedData;

            var foreignKey = attr.IdentifiablePropertyName;
            var foreignKeyProperty = entityProperties.FirstOrDefault(p => p.Name == foreignKey);
            if (foreignKeyProperty == null && rio == null)
                return entity;

            SetHasOneForeignKeyValue(entity, attr, foreignKeyProperty, rio);
            SetHasOneNavigationPropertyValue(entity, attr, rio, included);

            // recursive call ...
            if(included != null)
            {
                var navigationPropertyValue = attr.GetValue(entity);
                var resourceGraphEntity = _jsonApiContext.ResourceGraph.GetContextEntity(attr.Type);
                if(navigationPropertyValue != null && resourceGraphEntity != null)
                {
                    var includedResource = included.SingleOrDefault(r => r.Type == rio.Type && r.Id == rio.Id);
                    if(includedResource != null)
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
                if (rio == null && Nullable.GetUnderlyingType(foreignKeyProperty.PropertyType) == null)
                    throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"Cannot set required relationship identifier '{hasOneAttr.IdentifiablePropertyName}' to null because it is a non-nullable type."
                    });

                var convertedValue = TypeHelper.ConvertType(foreignKeyPropertyValue, foreignKeyProperty.PropertyType);
                foreignKeyProperty.SetValue(entity, convertedValue);
                _jsonApiContext.RelationshipsToUpdate[hasOneAttr] = convertedValue;
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

                // we need to store the fact that this relationship was included in the payload
                // for EF, the repository will use these pointers to make ensure we don't try to
                // create resources if they already exist, we just need to create the relationship
                _jsonApiContext.HasOneRelationshipPointers.Add(hasOneAttr, includedRelationshipObject);
            }
        }

        private object SetHasManyRelationship(object entity,
            PropertyInfo[] entityProperties,
            RelationshipAttribute attr,
            ContextEntity contextEntity,
            Dictionary<string, RelationshipData> relationships,
            List<ResourceObject> included = null)
        {
            var relationshipName = attr.PublicRelationshipName;

            if (relationships.TryGetValue(relationshipName, out RelationshipData relationshipData))
            {
                if(relationshipData.IsHasMany == false || relationshipData.ManyData == null)
                    return entity;

                var relatedResources = relationshipData.ManyData.Select(r =>
                {
                    var instance = GetIncludedRelationship(r, included, attr);
                    return instance;
                });

                var convertedCollection = TypeHelper.ConvertCollection(relatedResources, attr.Type);

                attr.SetValue(entity, convertedCollection);

                _jsonApiContext.HasManyRelationshipPointers.Add(attr, convertedCollection);
            }

            return entity;
        }

        private IIdentifiable GetIncludedRelationship(ResourceIdentifierObject relatedResourceIdentifier, List<ResourceObject> includedResources, RelationshipAttribute relationshipAttr)
        {
            // at this point we can be sure the relationshipAttr.Type is IIdentifiable because we were able to successfully build the ResourceGraph
            var relatedInstance = relationshipAttr.Type.New<IIdentifiable>();
            relatedInstance.StringId = relatedResourceIdentifier.Id;

            // can't provide any more data other than the rio since it is not contained in the included section
            if (includedResources == null || includedResources.Count == 0)
                return relatedInstance;

            var includedResource = GetLinkedResource(relatedResourceIdentifier, includedResources);
            if (includedResource == null)
                return relatedInstance;

            var contextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(relationshipAttr.Type);
            if (contextEntity == null)
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"Included type '{relationshipAttr.Type}' is not a registered json:api resource."
                    });

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
                throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
                {
                    Title = $"A compound document MUST NOT include more than one resource object for each type and id pair."
                            + $"The duplicate pair was '{relatedResourceIdentifier.Type}, {relatedResourceIdentifier.Id}'",
                    Source = e
                });
            }
        }
    }
}
