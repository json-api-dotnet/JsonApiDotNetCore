using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

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
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly ISparseFieldSetCache _sparseFieldSetCache;
        private readonly IJsonApiOptions _options;
        private readonly Type _primaryResourceType;

        /// <inheritdoc />
        public string ContentType { get; } = HeaderConstants.MediaType;

        public ResponseSerializer(IMetaBuilder metaBuilder, ILinkBuilder linkBuilder, IIncludedResourceObjectBuilder includedBuilder,
            IFieldsToSerialize fieldsToSerialize, IResourceObjectBuilder resourceObjectBuilder, IResourceDefinitionAccessor resourceDefinitionAccessor,
            ISparseFieldSetCache sparseFieldSetCache, IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            ArgumentGuard.NotNull(metaBuilder, nameof(metaBuilder));
            ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
            ArgumentGuard.NotNull(includedBuilder, nameof(includedBuilder));
            ArgumentGuard.NotNull(fieldsToSerialize, nameof(fieldsToSerialize));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(sparseFieldSetCache, nameof(sparseFieldSetCache));
            ArgumentGuard.NotNull(options, nameof(options));

            _metaBuilder = metaBuilder;
            _linkBuilder = linkBuilder;
            _includedBuilder = includedBuilder;
            _fieldsToSerialize = fieldsToSerialize;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _sparseFieldSetCache = sparseFieldSetCache;
            _options = options;
            _primaryResourceType = typeof(TResource);
        }

        /// <inheritdoc />
        public string Serialize(object content)
        {
            _sparseFieldSetCache.Reset();

            if (content is null or IIdentifiable)
            {
                return SerializeSingle((IIdentifiable)content);
            }

            if (content is IEnumerable<IIdentifiable> collectionOfIdentifiable)
            {
                return SerializeMany(collectionOfIdentifiable.ToArray());
            }

            if (content is IEnumerable<ErrorObject> errorObjects)
            {
                var errorDocument = new Document
                {
                    Errors = errorObjects.ToArray()
                };

                return SerializeErrorDocument(errorDocument);
            }

            if (content is ErrorObject errorObject)
            {
                var errorDocument = new Document
                {
                    Errors = errorObject.AsArray()
                };

                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be null, errors or resources.");
        }

        private string SerializeErrorDocument(Document document)
        {
            SetApiVersion(document);

            return SerializeObject(document, _options.SerializerWriteOptions);
        }

        /// <summary>
        /// Converts a single resource into a serialized <see cref="Document" />.
        /// </summary>
        /// <remarks>
        /// This method is internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeSingle(IIdentifiable resource)
        {
            if (resource != null && _fieldsToSerialize.ShouldSerialize)
            {
                _resourceDefinitionAccessor.OnSerialize(resource);
            }

            IReadOnlyCollection<AttrAttribute> attributes = _fieldsToSerialize.GetAttributes(_primaryResourceType);
            IReadOnlyCollection<RelationshipAttribute> relationships = _fieldsToSerialize.GetRelationships(_primaryResourceType);

            Document document = Build(resource, attributes, relationships);
            ResourceObject resourceObject = document.Data.SingleValue;

            if (resourceObject != null)
            {
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerWriteOptions);
        }

        /// <summary>
        /// Converts a collection of resources into a serialized <see cref="Document" />.
        /// </summary>
        /// <remarks>
        /// This method is internal instead of private for easier testability.
        /// </remarks>
        internal string SerializeMany(IReadOnlyCollection<IIdentifiable> resources)
        {
            if (_fieldsToSerialize.ShouldSerialize)
            {
                foreach (IIdentifiable resource in resources)
                {
                    _resourceDefinitionAccessor.OnSerialize(resource);
                }
            }

            IReadOnlyCollection<AttrAttribute> attributes = _fieldsToSerialize.GetAttributes(_primaryResourceType);
            IReadOnlyCollection<RelationshipAttribute> relationships = _fieldsToSerialize.GetRelationships(_primaryResourceType);

            Document document = Build(resources, attributes, relationships);

            foreach (ResourceObject resourceObject in document.Data.ManyValue)
            {
                ResourceLinks links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);

                if (links == null)
                {
                    break;
                }

                resourceObject.Links = links;
            }

            AddTopLevelObjects(document);

            return SerializeObject(document, _options.SerializerWriteOptions);
        }

        /// <summary>
        /// Adds top-level objects that are only added to a document in the case of server-side serialization.
        /// </summary>
        private void AddTopLevelObjects(Document document)
        {
            SetApiVersion(document);

            document.Links = _linkBuilder.GetTopLevelLinks();
            document.Meta = _metaBuilder.Build();
            document.Included = _includedBuilder.Build();
        }

        private void SetApiVersion(Document document)
        {
            if (_options.IncludeJsonApiVersion)
            {
                document.JsonApi = new JsonApiObject
                {
                    Version = "1.1"
                };
            }
        }
    }
}
