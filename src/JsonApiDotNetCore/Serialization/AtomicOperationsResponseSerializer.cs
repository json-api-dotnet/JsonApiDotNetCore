using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server serializer implementation of <see cref="BaseSerializer"/> for atomic:operations requests.
    /// </summary>
    public sealed class AtomicOperationsResponseSerializer : BaseSerializer, IJsonApiSerializer
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IJsonApiOptions _options;

        public string ContentType { get; } = HeaderConstants.AtomicOperationsMediaType;

        public AtomicOperationsResponseSerializer(IResourceObjectBuilder resourceObjectBuilder,
            IResourceContextProvider resourceContextProvider, IJsonApiOptions options)
            : base(resourceObjectBuilder)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
                    resourceObject = ResourceObjectBuilder.Build(resource, resourceContext.Attributes, resourceContext.Relationships);
                }

                document.Results.Add(new AtomicResultObject
                {
                    Data = resourceObject
                });
            }

            return SerializeObject(document, _options.SerializerSettings);
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer => { serializer.ApplyErrorSettings(); });
        }
    }
}
