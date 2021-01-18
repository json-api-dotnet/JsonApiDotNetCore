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
        private readonly IJsonApiOptions _options;
        private readonly IMetaBuilder _metaBuilder;

        public string ContentType { get; } = HeaderConstants.AtomicOperationsMediaType;

        public AtomicOperationsResponseSerializer(IResourceObjectBuilder resourceObjectBuilder, ILinkBuilder linkBuilder,
            IResourceContextProvider resourceContextProvider, IJsonApiOptions options, IMetaBuilder metaBuilder)
            : base(resourceObjectBuilder)
        {
            _linkBuilder = linkBuilder ?? throw new ArgumentNullException(nameof(linkBuilder));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _metaBuilder = metaBuilder ?? throw new ArgumentNullException(nameof(metaBuilder));
        }

        public string Serialize(object content)
        {
            if (content is IList<IIdentifiable> resources)
            {
                return SerializeOperationsDocument(resources);
            }

            if (content is ErrorDocument errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or an atomic:operations document.");
        }

        private string SerializeOperationsDocument(IEnumerable<IIdentifiable> resources)
        {
            var document = new AtomicOperationsDocument
            {
                Results = new List<AtomicResultObject>()
            };

            foreach (IIdentifiable resource in resources)
            {
                ResourceObject resourceObject = null;

                if (resource != null)
                {
                    var resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());

                    // TODO: @OPS: Should inject IFieldsToSerialize, which uses SparseFieldSetCache to call into resource definitions to hide fields.
                    // But then we need to update IJsonApiRequest for each loop entry, which we don't have access to anymore.
                    // That would be more correct, because ILinkBuilder depends on IJsonApiRequest too.

                    var attributes = resourceContext.Attributes
                        .Where(attr => attr.Capabilities.HasFlag(AttrCapabilities.AllowView))
                        .ToArray();

                    resourceObject = ResourceObjectBuilder.Build(resource, attributes, resourceContext.Relationships);
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
