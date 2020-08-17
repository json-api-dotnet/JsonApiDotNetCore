using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCore.Serialization.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Abstract base class for deserialization. Deserializes JSON content into <see cref="Document"/>s
    /// And constructs instances of the resource(s) in the document body.
    /// </summary>
    public abstract class BaseDocumentParser
    {
        protected readonly IResourceContextProvider _contextProvider;
        protected readonly IResourceFactory _resourceFactory;
        protected Document _document;

        protected BaseDocumentParser(IResourceContextProvider contextProvider, IResourceFactory resourceFactory)
        {
            _contextProvider = contextProvider;
            _resourceFactory = resourceFactory;
        }

        /// <summary>
        /// This method is called each time an <paramref name="resource"/> is constructed
        /// from the serialized content, which is used to do additional processing
        /// depending on the type of deserializers.
        /// </summary>
        /// <remarks>
        /// See the implementation of this method in <see cref="ResponseDeserializer"/>
        /// and <see cref="RequestDeserializer"/> for examples.
        /// </remarks>
        /// <param name="resource">The resource that was constructed from the document's body</param>
        /// <param name="field">The metadata for the exposed field</param>
        /// <param name="data">Relationship data for <paramref name="resource"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/></param>
        protected abstract void AfterProcessField(IIdentifiable resource, ResourceFieldAttribute field, RelationshipEntry data = null);

        /// <inheritdoc/>
        protected object Deserialize(string body)
        {
            var bodyJToken = LoadJToken(body);
            _document = bodyJToken.ToObject<Document>();
            if (_document.IsManyData)
            {
                if (_document.ManyData.Count == 0)
                    return Array.Empty<IIdentifiable>();

                return _document.ManyData.Select(ParseResourceObject).ToList();
            }

            if (_document.SingleData == null) return null;
            return ParseResourceObject(_document.SingleData);
        }

        /// <summary>
        /// Sets the attributes on a parsed resource.
        /// </summary>
        /// <param name="resource">The parsed resource</param>
        /// <param name="attributeValues">Attributes and their values, as in the serialized content</param>
        /// <param name="attributes">Exposed attributes for <paramref name="resource"/></param>
        /// <returns></returns>
        protected virtual IIdentifiable SetAttributes(IIdentifiable resource, Dictionary<string, object> attributeValues, List<AttrAttribute> attributes)
        {
            if (attributeValues == null || attributeValues.Count == 0)
                return resource;

            foreach (var attr in attributes)
            {
                if (attributeValues.TryGetValue(attr.PublicName, out object newValue))
                {
                    var convertedValue = ConvertAttrValue(newValue, attr.Property.PropertyType);
                    attr.SetValue(resource, convertedValue);
                    AfterProcessField(resource, attr);
                }
            }

            return resource;
        }

        /// <summary>
        /// Sets the relationships on a parsed resource
        /// </summary>
        /// <param name="resource">The parsed resource</param>
        /// <param name="relationshipsValues">Relationships and their values, as in the serialized content</param>
        /// <param name="relationshipAttributes">Exposed relationships for <paramref name="resource"/></param>
        /// <returns></returns>
        protected virtual IIdentifiable SetRelationships(IIdentifiable resource, Dictionary<string, RelationshipEntry> relationshipsValues, List<RelationshipAttribute> relationshipAttributes)
        {
            if (relationshipsValues == null || relationshipsValues.Count == 0)
                return resource;

            var resourceProperties = resource.GetType().GetProperties();
            foreach (var attr in relationshipAttributes)
            {
                if (!relationshipsValues.TryGetValue(attr.PublicName, out RelationshipEntry relationshipData) || !relationshipData.IsPopulated)
                    continue;

                if (attr is HasOneAttribute hasOneAttribute)
                    SetHasOneRelationship(resource, resourceProperties, hasOneAttribute, relationshipData);
                else
                    SetHasManyRelationship(resource, (HasManyAttribute)attr, relationshipData);
            }
            return resource;
        }

        private JToken LoadJToken(string body)
        {
            JToken jToken;
            using (JsonReader jsonReader = new JsonTextReader(new StringReader(body)))
            {
                // https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/509
                jsonReader.DateParseHandling = DateParseHandling.None;
                jToken = JToken.Load(jsonReader);
            }
            return jToken;
        }

        /// <summary>
        /// Creates an instance of the referenced type in <paramref name="data"/>
        /// and sets its attributes and relationships
        /// </summary>
        /// <param name="data"></param>
        /// <returns>The parsed resource</returns>
        private IIdentifiable ParseResourceObject(ResourceObject data)
        {
            var resourceContext = _contextProvider.GetResourceContext(data.Type);
            if (resourceContext == null)
            {
                throw new InvalidRequestBodyException("Payload includes unknown resource type.",
                    $"The resource '{data.Type}' is not registered on the resource graph. " +
                    "If you are using Entity Framework Core, make sure the DbSet matches the expected resource name. " +
                    "If you have manually registered the resource, check that the call to AddResource correctly sets the public name.", null);
            }

            var resource = (IIdentifiable)_resourceFactory.CreateInstance(resourceContext.ResourceType);

            resource = SetAttributes(resource, data.Attributes, resourceContext.Attributes);
            resource = SetRelationships(resource, data.Relationships, resourceContext.Relationships);

            if (data.Id != null)
                resource.StringId = data.Id;

            return resource;
        }

        /// <summary>
        /// Sets a HasOne relationship on a parsed resource. If present, also
        /// populates the foreign key.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="resourceProperties"></param>
        /// <param name="attr"></param>
        /// <param name="relationshipData"></param>
        private void SetHasOneRelationship(IIdentifiable resource,
            PropertyInfo[] resourceProperties,
            HasOneAttribute attr,
            RelationshipEntry relationshipData)
        {
            var rio = (ResourceIdentifierObject)relationshipData.Data;
            var relatedId = rio?.Id;

            // this does not make sense in the following case: if we're setting the dependent of a one-to-one relationship, IdentifiablePropertyName should be null.
            var foreignKeyProperty = resourceProperties.FirstOrDefault(p => p.Name == attr.IdentifiablePropertyName);

            if (foreignKeyProperty != null)
                // there is a FK from the current resource pointing to the related object,
                // i.e. we're populating the relationship from the dependent side.
                SetForeignKey(resource, foreignKeyProperty, attr, relatedId);

            SetNavigation(resource, attr, relatedId);

            // depending on if this base parser is used client-side or server-side,
            // different additional processing per field needs to be executed.
            AfterProcessField(resource, attr, relationshipData);
        }

        /// <summary>
        /// Sets the dependent side of a HasOne relationship, which means that a
        /// foreign key also will to be populated.
        /// </summary>
        private void SetForeignKey(IIdentifiable resource, PropertyInfo foreignKey, HasOneAttribute attr, string id)
        {
            bool foreignKeyPropertyIsNullableType = Nullable.GetUnderlyingType(foreignKey.PropertyType) != null
                || foreignKey.PropertyType == typeof(string);
            if (id == null && !foreignKeyPropertyIsNullableType)
            {
                // this happens when a non-optional relationship is deliberately set to null.
                // For a server deserializer, it should be mapped to a BadRequest HTTP error code.
                throw new FormatException($"Cannot set required relationship identifier '{attr.IdentifiablePropertyName}' to null because it is a non-nullable type.");
            }

            var typedId = TypeHelper.ConvertStringIdToTypedId(attr.Property.PropertyType, id, _resourceFactory);
            foreignKey.SetValue(resource, typedId);
        }

        /// <summary>
        /// Sets the principal side of a HasOne relationship, which means no
        /// foreign key is involved
        /// </summary>
        private void SetNavigation(IIdentifiable resource, HasOneAttribute attr, string relatedId)
        {
            if (relatedId == null)
            {
                attr.SetValue(resource, null, _resourceFactory);
            }
            else
            {
                var relatedInstance = (IIdentifiable)_resourceFactory.CreateInstance(attr.RightType);
                relatedInstance.StringId = relatedId;
                attr.SetValue(resource, relatedInstance, _resourceFactory);
            }
        }

        /// <summary>
        /// Sets a HasMany relationship.
        /// </summary>
        private void SetHasManyRelationship(
            IIdentifiable resource,
            HasManyAttribute attr,
            RelationshipEntry relationshipData)
        {
            if (relationshipData.Data != null)
            {   // if the relationship is set to null, no need to set the navigation property to null: this is the default value.
                var relatedResources = relationshipData.ManyData.Select(rio =>
                {
                    var relatedInstance = (IIdentifiable)_resourceFactory.CreateInstance(attr.RightType);
                    relatedInstance.StringId = rio.Id;
                    return relatedInstance;
                });

                var convertedCollection = relatedResources.CopyToTypedCollection(attr.Property.PropertyType);
                attr.SetValue(resource, convertedCollection, _resourceFactory);
            }

            AfterProcessField(resource, attr, relationshipData);
        }

        private object ConvertAttrValue(object newValue, Type targetType)
        {
            if (newValue is JContainer jObject)
                // the attribute value is a complex type that needs additional deserialization
                return DeserializeComplexType(jObject, targetType);

            // the attribute value is a native C# type.
            var convertedValue = TypeHelper.ConvertType(newValue, targetType);
            return convertedValue;
        }

        private object DeserializeComplexType(JContainer obj, Type targetType)
        {
            return obj.ToObject(targetType);
        }
    }
}
