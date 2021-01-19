using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server serializer implementation of <see cref="BaseSerializer"/> for atomic:operations requests.
    /// </summary>
    public sealed class AtomicOperationsResponseSerializer : BaseSerializer, IJsonApiSerializer
    {
        private readonly ILinkBuilder _linkBuilder;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly IJsonApiRequest _request;
        private readonly IJsonApiOptions _options;
        private readonly IMetaBuilder _metaBuilder;

        public string ContentType { get; } = HeaderConstants.AtomicOperationsMediaType;

        public AtomicOperationsResponseSerializer(IMetaBuilder metaBuilder, ILinkBuilder linkBuilder,
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider,
            IFieldsToSerialize fieldsToSerialize, IJsonApiRequest request,
            IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            _linkBuilder = linkBuilder ?? throw new ArgumentNullException(nameof(linkBuilder));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _fieldsToSerialize = fieldsToSerialize ?? throw new ArgumentNullException(nameof(fieldsToSerialize));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _metaBuilder = metaBuilder ?? throw new ArgumentNullException(nameof(metaBuilder));
        }

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

            throw new InvalidOperationException("Data being returned must be errors or an atomic:operations document.");
        }

        private string SerializeOperationsDocument(IEnumerable<OperationContainer> operations)
        {
            var document = new AtomicOperationsDocument
            {
                Results = new List<AtomicResultObject>()
            };

            foreach (var operation in operations)
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
                    if (resourceObject != null)
                    {
                        resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                    }
                }

                document.Results.Add(new AtomicResultObject
                {
                    Data = resourceObject
                });
            }

            document.Meta = _metaBuilder.Build();

            return SerializeObject(document, _options.SerializerSettings);
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer => { serializer.ApplyErrorSettings(); });
        }
    }
}
