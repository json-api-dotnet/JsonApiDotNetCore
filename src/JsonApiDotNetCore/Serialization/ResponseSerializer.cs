using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Server serializer implementation of <see cref="BaseSerializer"/> for resources of a specific type.
    /// </summary>
    /// <remarks>
    /// Because in JsonApiDotNetCore every JSON:API request is associated with exactly one
    /// resource (the primary resource, see <see cref="IJsonApiRequest.PrimaryResource"/>),
    /// the serializer can leverage this information using generics.
    /// See <see cref="ResponseSerializerFactory"/> for how this is instantiated.
    /// </remarks>
    /// <typeparam name="TResource">Type of the resource associated with the scope of the request
    /// for which this serializer is used.</typeparam>
    public class ResponseSerializer<TResource> : BaseSerializer, IJsonApiSerializer
        where TResource : class, IIdentifiable
    {
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IJsonApiOptions _options;
        private readonly IMetaBuilder _metaBuilder;
        private readonly Type _primaryResourceType;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IIncludedResourceObjectBuilder _includedBuilder;

        /// <inheritdoc />
        public string ContentType { get; } = HeaderConstants.MediaType;

        public ResponseSerializer(IMetaBuilder metaBuilder,
            ILinkBuilder linkBuilder,
            IIncludedResourceObjectBuilder includedBuilder,
            IFieldsToSerialize fieldsToSerialize,
            IResourceObjectBuilder resourceObjectBuilder,
            IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            _fieldsToSerialize = fieldsToSerialize ?? throw new ArgumentNullException(nameof(fieldsToSerialize));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _linkBuilder = linkBuilder ?? throw new ArgumentNullException(nameof(linkBuilder));
            _metaBuilder = metaBuilder ?? throw new ArgumentNullException(nameof(metaBuilder));
            _includedBuilder = includedBuilder ?? throw new ArgumentNullException(nameof(includedBuilder));
            _primaryResourceType = typeof(TResource);
        }

        /// <inheritdoc />
        public string Serialize(object data)
        {
            if (data == null || data is IIdentifiable)
            {
                return SerializeSingle((IIdentifiable)data);
            }

            if (data is IEnumerable<IIdentifiable> collectionOfIdentifiable)
            {
                return SerializeMany(collectionOfIdentifiable.ToArray());
            }

            if (data is ErrorDocument errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or resources.");
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer => { serializer.ApplyErrorSettings(); });
        }

        /// <summary>
        /// Converts a single resource into a serialized <see cref="Document"/>.
        /// </summary>
        /// <remarks>
        /// This method is internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeSingle(IIdentifiable resource)
        {
            var (attributes, relationships) = GetFieldsToSerialize();
            var document = Build(resource, attributes, relationships);
            var resourceObject = document.SingleData;
            if (resourceObject != null)
            {
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerSettings, serializer => { serializer.NullValueHandling = NullValueHandling.Include; });
        }

        private (IReadOnlyCollection<AttrAttribute>, IReadOnlyCollection<RelationshipAttribute>) GetFieldsToSerialize()
        {
            return (_fieldsToSerialize.GetAttributes(_primaryResourceType), _fieldsToSerialize.GetRelationships(_primaryResourceType));
        }

        /// <summary>
        /// Converts a collection of resources into a serialized <see cref="Document"/>.
        /// </summary>
        /// <remarks>
        /// This method is internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeMany(IReadOnlyCollection<IIdentifiable> resources)
        {
            var (attributes, relationships) = GetFieldsToSerialize();
            var document = Build(resources, attributes, relationships);
            foreach (ResourceObject resourceObject in document.ManyData)
            {
                var links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                if (links == null)
                {
                    break;
                }

                resourceObject.Links = links;
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerSettings, serializer => { serializer.NullValueHandling = NullValueHandling.Include; });
        }

        /// <summary>
        /// Adds top-level objects that are only added to a document in the case
        /// of server-side serialization.
        /// </summary>
        private void AddTopLevelObjects(Document document)
        {
            document.Links = _linkBuilder.GetTopLevelLinks();
            document.Meta = _metaBuilder.Build();
            document.Included = _includedBuilder.Build();
        }
    }
}
