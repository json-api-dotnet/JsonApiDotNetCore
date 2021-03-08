using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server serializer implementation of <see cref="BaseSerializer" /> for resources of a specific type.
    /// </summary>
    /// <remarks>
    /// Because in JsonApiDotNetCore every JSON:API request is associated with exactly one resource (the primary resource, see
    /// <see cref="IJsonApiRequest.PrimaryResource" />), the serializer can leverage this information using generics. See
    /// <see cref="ResponseSerializerFactory" /> for how this is instantiated.
    /// </remarks>
    /// <typeparam name="TResource">
    /// Type of the resource associated with the scope of the request for which this serializer is used.
    /// </typeparam>
    [PublicAPI]
    public class ResponseSerializer<TResource> : BaseSerializer, IJsonApiSerializer
        where TResource : class, IIdentifiable
    {
        private readonly IMetaBuilder _metaBuilder;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IIncludedResourceObjectBuilder _includedBuilder;
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IJsonApiOptions _options;
        private readonly Type _primaryResourceType;

        /// <inheritdoc />
        public string ContentType { get; } = HeaderConstants.MediaType;

        public ResponseSerializer(IMetaBuilder metaBuilder, ILinkBuilder linkBuilder, IIncludedResourceObjectBuilder includedBuilder,
            IFieldsToSerialize fieldsToSerialize, IResourceObjectBuilder resourceObjectBuilder, IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            ArgumentGuard.NotNull(metaBuilder, nameof(metaBuilder));
            ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
            ArgumentGuard.NotNull(includedBuilder, nameof(includedBuilder));
            ArgumentGuard.NotNull(fieldsToSerialize, nameof(fieldsToSerialize));
            ArgumentGuard.NotNull(options, nameof(options));

            _metaBuilder = metaBuilder;
            _linkBuilder = linkBuilder;
            _includedBuilder = includedBuilder;
            _fieldsToSerialize = fieldsToSerialize;
            _options = options;
            _primaryResourceType = typeof(TResource);
        }

        /// <inheritdoc />
        public string Serialize(object content)
        {
            if (content == null || content is IIdentifiable)
            {
                return SerializeSingle((IIdentifiable)content);
            }

            if (content is IEnumerable<IIdentifiable> collectionOfIdentifiable)
            {
                return SerializeMany(collectionOfIdentifiable.ToArray());
            }

            if (content is ErrorDocument errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or resources.");
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer =>
            {
                serializer.ApplyErrorSettings();
            });
        }

        /// <summary>
        /// Converts a single resource into a serialized <see cref="Document" />.
        /// </summary>
        /// <remarks>
        /// This method is internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeSingle(IIdentifiable resource)
        {
            IReadOnlyCollection<AttrAttribute> attributes = _fieldsToSerialize.GetAttributes(_primaryResourceType);
            IReadOnlyCollection<RelationshipAttribute> relationships = _fieldsToSerialize.GetRelationships(_primaryResourceType);

            Document document = Build(resource, attributes, relationships);
            ResourceObject resourceObject = document.SingleData;

            if (resourceObject != null)
            {
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerSettings, serializer =>
            {
                serializer.NullValueHandling = NullValueHandling.Include;
            });
        }

        /// <summary>
        /// Converts a collection of resources into a serialized <see cref="Document" />.
        /// </summary>
        /// <remarks>
        /// This method is internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeMany(IReadOnlyCollection<IIdentifiable> resources)
        {
            IReadOnlyCollection<AttrAttribute> attributes = _fieldsToSerialize.GetAttributes(_primaryResourceType);
            IReadOnlyCollection<RelationshipAttribute> relationships = _fieldsToSerialize.GetRelationships(_primaryResourceType);

            Document document = Build(resources, attributes, relationships);

            foreach (ResourceObject resourceObject in document.ManyData)
            {
                ResourceLinks links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);

                if (links == null)
                {
                    break;
                }

                resourceObject.Links = links;
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerSettings, serializer =>
            {
                serializer.NullValueHandling = NullValueHandling.Include;
            });
        }

        /// <summary>
        /// Adds top-level objects that are only added to a document in the case of server-side serialization.
        /// </summary>
        private void AddTopLevelObjects(Document document)
        {
            document.Links = _linkBuilder.GetTopLevelLinks();
            document.Meta = _metaBuilder.Build();
            document.Included = _includedBuilder.Build();
        }
    }
}
