using System;
using System.Linq;
using System.Net.Http;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
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
        private readonly IJsonApiRequest _request;

        public RequestDeserializer(
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory,
            ITargetedFields targetedFields,
            IHttpContextAccessor httpContextAccessor,
            IJsonApiRequest request) 
            : base(resourceContextProvider, resourceFactory)
        {
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <inheritdoc />
        public object Deserialize(string body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));

            if (_request.Kind == EndpointKind.Relationship)
            {
                _targetedFields.Relationships.Add(_request.Relationship);
            }
            
            var instance = DeserializeBody(body);

            AssertResourceIdIsNotTargeted();
            
            return instance;
        }

        private void AssertResourceIdIsNotTargeted()
        {
            if (!_request.IsReadOnly && _targetedFields.Attributes.Any(attribute => attribute.Property.Name == nameof(Identifiable.Id)))
            {
                throw new JsonApiSerializationException("Resource ID is read-only.", null);
            }
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
                    throw new JsonApiSerializationException(
                        "Setting the initial value of the requested attribute is not allowed.",
                        $"Setting the initial value of '{attr.PublicName}' is not allowed.");
                }

                if (_httpContextAccessor.HttpContext.Request.Method == HttpMethod.Patch.Method &&
                    !attr.Capabilities.HasFlag(AttrCapabilities.AllowChange))
                {
                    throw new JsonApiSerializationException(
                        "Changing the value of the requested attribute is not allowed.",
                        $"Changing the value of '{attr.PublicName}' is not allowed.");
                }

                _targetedFields.Attributes.Add(attr);
            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }
    }
}
