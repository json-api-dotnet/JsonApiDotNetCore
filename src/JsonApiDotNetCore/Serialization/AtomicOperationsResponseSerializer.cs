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
    /// Server serializer implementation of <see cref="BaseSerializer" /> for atomic:operations responses.
    /// </summary>
    [PublicAPI]
    public sealed class AtomicOperationsResponseSerializer : BaseSerializer, IJsonApiSerializer
    {
        private readonly IMetaBuilder _metaBuilder;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
        private readonly IJsonApiRequest _request;
        private readonly IJsonApiOptions _options;

        /// <inheritdoc />
        public string ContentType { get; } = HeaderConstants.AtomicOperationsMediaType;

        public AtomicOperationsResponseSerializer(IResourceObjectBuilder resourceObjectBuilder, IMetaBuilder metaBuilder, ILinkBuilder linkBuilder,
            IFieldsToSerialize fieldsToSerialize, IResourceDefinitionAccessor resourceDefinitionAccessor, IEvaluatedIncludeCache evaluatedIncludeCache,
            IJsonApiRequest request, IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            ArgumentGuard.NotNull(metaBuilder, nameof(metaBuilder));
            ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
            ArgumentGuard.NotNull(fieldsToSerialize, nameof(fieldsToSerialize));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(evaluatedIncludeCache, nameof(evaluatedIncludeCache));
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(options, nameof(options));

            _metaBuilder = metaBuilder;
            _linkBuilder = linkBuilder;
            _fieldsToSerialize = fieldsToSerialize;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _evaluatedIncludeCache = evaluatedIncludeCache;
            _request = request;
            _options = options;
        }

        /// <inheritdoc />
        public string Serialize(object content)
        {
            if (content is IList<OperationContainer> operations)
            {
                return SerializeOperationsDocument(operations);
            }

            if (content is Document errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or operations.");
        }

        private string SerializeOperationsDocument(IEnumerable<OperationContainer> operations)
        {
            var document = new Document
            {
                Results = operations.Select(SerializeOperation).ToList(),
                Meta = _metaBuilder.Build()
            };

            SetApiVersion(document);

            return SerializeObject(document, _options.SerializerWriteOptions);
        }

        private void SetApiVersion(Document document)
        {
            if (_options.IncludeJsonApiVersion)
            {
                document.JsonApi = new JsonApiObject
                {
                    Version = "1.1",
                    Ext = new List<string>
                    {
                        "https://jsonapi.org/ext/atomic"
                    }
                };
            }
        }

        private AtomicResultObject SerializeOperation(OperationContainer operation)
        {
            ResourceObject resourceObject = null;

            if (operation != null)
            {
                _request.CopyFrom(operation.Request);
                _fieldsToSerialize.ResetCache();
                _evaluatedIncludeCache.Set(null);

                _resourceDefinitionAccessor.OnSerialize(operation.Resource);

                Type resourceType = operation.Resource.GetType();
                IReadOnlyCollection<AttrAttribute> attributes = _fieldsToSerialize.GetAttributes(resourceType);
                IReadOnlyCollection<RelationshipAttribute> relationships = _fieldsToSerialize.GetRelationships(resourceType);

                resourceObject = ResourceObjectBuilder.Build(operation.Resource, attributes, relationships);
            }

            if (resourceObject != null)
            {
                resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
            }

            return new AtomicResultObject
            {
                Data = new SingleOrManyData<ResourceObject>(resourceObject)
            };
        }

        private string SerializeErrorDocument(Document document)
        {
            SetApiVersion(document);

            return SerializeObject(document, _options.SerializerWriteOptions);
        }
    }
}
