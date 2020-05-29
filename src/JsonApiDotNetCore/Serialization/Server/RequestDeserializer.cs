using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
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
        protected override void AfterProcessField(IIdentifiable entity, ResourceFieldAttribute field, RelationshipEntry data = null)
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
                        $"Changing the value of '{attr.PublicName}' is not allowed.", null);
                }
            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }

        protected override IIdentifiable SetAttributes(IIdentifiable entity, Dictionary<string, object> attributeValues, List<AttrAttribute> attributes)
        {
            if (_httpContextAccessor.HttpContext.Request.Method == HttpMethod.Patch.Method)
            {
                foreach (AttrAttribute attr in attributes)
                {
                    if (attr.PropertyInfo.GetCustomAttribute<IsRequiredAttribute>() != null)
                    {
                        bool disableValidator = attributeValues == null || attributeValues.Count == 0 ||
                                                !attributeValues.TryGetValue(attr.PublicAttributeName, out _);

                        if (disableValidator)
                        {
                            _httpContextAccessor.HttpContext.DisableValidator(attr.PropertyInfo.Name, entity.GetType().Name);
                        }
                    }
                }
            }

            return base.SetAttributes(entity, attributeValues, attributes);
        }

        protected override IIdentifiable SetRelationships(IIdentifiable entity, Dictionary<string, RelationshipEntry> relationshipsValues, List<RelationshipAttribute> relationshipAttributes)
        {
            // If there is a relationship included in the data of the POST or PATCH, then the 'IsRequired' attribute will be disabled for any
            // property within that object. For instance, a new article is posted and has a relationship included to an author. In this case,
            // the author name (which has the 'IsRequired' attribute) will not be included in the POST. Unless disabled, the POST will fail.
            foreach (RelationshipAttribute attr in relationshipAttributes)
            {
                _httpContextAccessor.HttpContext.DisableValidator("Relation", attr.PropertyInfo.Name);
            }

            return base.SetRelationships(entity, relationshipsValues, relationshipAttributes);
        }
    }
}
