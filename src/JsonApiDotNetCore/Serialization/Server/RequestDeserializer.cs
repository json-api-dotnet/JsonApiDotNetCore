using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models.CustomValidators;
using System.Net.Http;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// Server deserializer implementation of the <see cref="BaseDocumentParser"/>
    /// </summary>
    public class RequestDeserializer : BaseDocumentParser, IJsonApiDeserializer
    {
        private readonly ITargetedFields  _targetedFields;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestDeserializer(IResourceContextProvider contextProvider, IResourceFactory resourceFactory, ITargetedFields  targetedFields, IHttpContextAccessor httpContextAccessor) 
            : base(contextProvider, resourceFactory)
        {
            _targetedFields = targetedFields;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public new object Deserialize(string body)
        {
            return base.Deserialize(body);
        }

        /// <summary>
        /// Additional processing required for server deserialization. Flags a
        /// processed attribute or relationship as updated using <see cref="ITargetedFields"/>.
        /// </summary>
        /// <param name="entity">The entity that was constructed from the document's body</param>
        /// <param name="field">The metadata for the exposed field</param>
        /// <param name="data">Relationship data for <paramref name="entity"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/></param>
        protected override void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipEntry data = null)
        {
            if (field is AttrAttribute attr)
            {
                if (attr.Capabilities.HasFlag(AttrCapabilities.AllowMutate))
                {
                    _targetedFields.Attributes.Add(attr);
                }
                else
                {
                    throw new InvalidRequestBodyException(
                        "Changing the value of the requested attribute is not allowed.",
                        $"Changing the value of '{attr.PublicAttributeName}' is not allowed.", null);
                }
            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }

        protected override IIdentifiable SetAttributes(IIdentifiable entity, Dictionary<string, object> attributeValues, List<AttrAttribute> attributes)
        {
            foreach (AttrAttribute attr in attributes)
            {
                var disableValidator = false;
                if (attributeValues == null || attributeValues.Count == 0)
                {
                    disableValidator = true;
                }
                else
                {
                    if (attributeValues.TryGetValue(attr.PublicAttributeName, out object newValue))
                    {
                        object convertedValue = ConvertAttrValue(newValue, attr.PropertyInfo.PropertyType);
                        attr.SetValue(entity, convertedValue);
                        AfterProcessField(entity, attr);
                    }
                    else
                    {
                        disableValidator = true;
                    }
                }

                if (!disableValidator) continue;
                if (_httpContextAccessor?.HttpContext?.Request.Method != HttpMethod.Patch.Method) continue;
                if (attr.PropertyInfo.GetCustomAttribute<IsRequiredAttribute>() != null)
                    _httpContextAccessor?.HttpContext.DisableValidator(attr.PropertyInfo.Name,
                        entity.GetType().Name);
            }

            return entity;
        }

        protected override IIdentifiable SetRelationships(IIdentifiable entity, Dictionary<string, RelationshipEntry> relationshipsValues, List<RelationshipAttribute> relationshipAttributes)
        {
            if (relationshipsValues == null || relationshipsValues.Count == 0)
                return entity;

            var entityProperties = entity.GetType().GetProperties();
            foreach (RelationshipAttribute attr in relationshipAttributes)
            {
                _httpContextAccessor?.HttpContext?.DisableValidator("Relation", attr.PropertyInfo.Name);

                if (!relationshipsValues.TryGetValue(attr.PublicRelationshipName, out RelationshipEntry relationshipData) || !relationshipData.IsPopulated)
                    continue;

                if (attr is HasOneAttribute hasOneAttribute)
                    SetHasOneRelationship(entity, entityProperties, hasOneAttribute, relationshipData);
                else
                    SetHasManyRelationship(entity, (HasManyAttribute)attr, relationshipData);
            }
            return entity;
        }
    }
}
