using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server serializer implementation of <see cref="BaseSerializer"/> for atomic:operations responses.
    /// </summary>
    public sealed class AtomicOperationsResponseSerializer : BaseSerializer, IJsonApiSerializer
    {
        private readonly IMetaBuilder _metaBuilder;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IJsonApiRequest _request;
        private readonly IJsonApiOptions _options;

        /// <inheritdoc />
        public string ContentType { get; } = HeaderConstants.AtomicOperationsMediaType;

        public AtomicOperationsResponseSerializer(IResourceObjectBuilder resourceObjectBuilder,
            IMetaBuilder metaBuilder, ILinkBuilder linkBuilder, IFieldsToSerialize fieldsToSerialize,
            IJsonApiRequest request, IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            _metaBuilder = metaBuilder ?? throw new ArgumentNullException(nameof(metaBuilder));
            _linkBuilder = linkBuilder ?? throw new ArgumentNullException(nameof(linkBuilder));
            _fieldsToSerialize = fieldsToSerialize ?? throw new ArgumentNullException(nameof(fieldsToSerialize));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public string Serialize(object content)
        {
            if (content is IList<OperationContainer> operations)
            {
                return SerializeOperationsDocument(operations);
            }

            if (content is ErrorDocument errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or operations.");
        }

        private string SerializeOperationsDocument(IEnumerable<OperationContainer> operations)
        {
            var document = new AtomicOperationsDocument
            {
                Results = operations.Select(SerializeOperation).ToList(),
                Meta = _metaBuilder.Build()
            };

            return SerializeObject(document, _options.SerializerSettings);
        }

        private AtomicResultObject SerializeOperation(OperationContainer operation)
        {
            ResourceObject resourceObject = null;

            if (operation != null)
            {
                _request.CopyFrom(operation.Request);
                _fieldsToSerialize.ResetCache();

                var resourceType = operation.Resource.GetType();
                var attributes = _fieldsToSerialize.GetAttributes(resourceType);
                var relationships = _fieldsToSerialize.GetRelationships(resourceType);

                resourceObject = ResourceObjectBuilder.Build(operation.Resource, attributes, relationships);
            }

            if (resourceObject != null)
            {
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
            }

            return new AtomicResultObject
            {
                Data = resourceObject
            };
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer => { serializer.ApplyErrorSettings(); });
        }
    }
}
