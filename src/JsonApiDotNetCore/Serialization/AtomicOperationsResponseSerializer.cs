using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server serializer implementation of <see cref="BaseSerializer"/> for atomic:operations requests.
    /// </summary>
    public sealed class AtomicOperationsResponseSerializer : BaseSerializer, IJsonApiSerializer
    {
        private readonly IJsonApiOptions _options;

        public string ContentType { get; } = HeaderConstants.AtomicOperationsMediaType;

        public AtomicOperationsResponseSerializer(IResourceObjectBuilder resourceObjectBuilder, IJsonApiOptions options) 
            : base(resourceObjectBuilder)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string Serialize(object content)
        {
            if (content is AtomicOperationsDocument atomicOperationsDocument)
            {
                return SerializeOperationsDocument(atomicOperationsDocument);
            }

            if (content is ErrorDocument errorDocument)
            {
                return SerializeErrorDocument(errorDocument);
            }

            throw new InvalidOperationException("Data being returned must be errors or an atomic:operations document.");
        }

        private string SerializeOperationsDocument(AtomicOperationsDocument content)
        {
            return SerializeObject(content, _options.SerializerSettings);
        }

        private string SerializeErrorDocument(ErrorDocument errorDocument)
        {
            return SerializeObject(errorDocument, _options.SerializerSettings, serializer => { serializer.ApplyErrorSettings(); });
        }
    }
}
