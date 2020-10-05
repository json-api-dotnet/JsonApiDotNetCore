using System;
using System.Net.Http;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server deserializer implementation of the <see cref="BaseDeserializer"/>.
    /// </summary>
    public class RequestDeserializer : BaseDeserializer, IJsonApiDeserializer
    {
        private readonly ITargetedFields  _targetedFields;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestDeserializer(IResourceContextProvider resourceContextProvider, IResourceFactory resourceFactory, ITargetedFields targetedFields, IHttpContextAccessor httpContextAccessor) 
            : base(resourceContextProvider, resourceFactory)
        {
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc />
        public object Deserialize(string body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));

            return DeserializeBody(body);
        }

        /// <summary>
        /// Additional processing required for server deserialization. Flags a
        /// processed attribute or relationship as updated using <see cref="ITargetedFields"/>.
        /// </summary>
        /// <param name="resource">The resource that was constructed from the document's body.</param>
        /// <param name="field">The metadata for the exposed field.</param>
        /// <param name="data">Relationship data for <paramref name="resource"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/>.</param>
        protected override void AfterProcessField(IIdentifiable resource, ResourceFieldAttribute field, RelationshipEntry data = null)
        {
            if (field is AttrAttribute attr)
            {
                if (_httpContextAccessor.HttpContext.Request.Method == HttpMethod.Post.Method &&
                    !attr.Capabilities.HasFlag(AttrCapabilities.AllowCreate))
                {
                    throw new InvalidRequestBodyException(
                        "Assigning to the requested attribute is not allowed.",
                        $"Assigning to '{attr.PublicName}' is not allowed.", null);
                }

                if (_httpContextAccessor.HttpContext.Request.Method == HttpMethod.Patch.Method &&
                    !attr.Capabilities.HasFlag(AttrCapabilities.AllowChange))
                {
                    throw new InvalidRequestBodyException(
                        "Changing the value of the requested attribute is not allowed.",
                        $"Changing the value of '{attr.PublicName}' is not allowed.", null);
                }

                _targetedFields.Attributes.Add(attr);
            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }
    }
}
