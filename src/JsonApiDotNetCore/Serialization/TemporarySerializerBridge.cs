using System;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Acts as a bridge between the legacy and new response serialization implementation. To be removed in a future commit, but handy for the moment to
    /// quickly toggle and see what breaks.
    /// </summary>
    public sealed class TemporarySerializerBridge : IJsonApiSerializer
    {
        private readonly IResponseModelAdapter _responseModelAdapter;
        private readonly IJsonApiSerializerFactory _factory;
        private readonly IJsonApiOptions _options;

        public string ContentType { get; private set; }

        public TemporarySerializerBridge(IResponseModelAdapter responseModelAdapter, IJsonApiSerializerFactory factory, IJsonApiOptions options)
        {
            ArgumentGuard.NotNull(responseModelAdapter, nameof(responseModelAdapter));
            ArgumentGuard.NotNull(factory, nameof(factory));
            ArgumentGuard.NotNull(options, nameof(options));

            _responseModelAdapter = responseModelAdapter;
            _factory = factory;
            _options = options;
        }

        public string Serialize(object model)
        {
            if (UseLegacySerializer(model))
            {
                IJsonApiSerializer serializer = _factory.GetSerializer();
                string responseBody = serializer.Serialize(model);
                ContentType = serializer.ContentType;
                return responseBody;
            }

            (Document document, string contentType) = _responseModelAdapter.Convert(model);
            ContentType = contentType;

            return SerializeObject(document, _options.SerializerWriteOptions);
        }

        private bool UseLegacySerializer(object model)
        {
            return false;
        }

        private string SerializeObject(object value, JsonSerializerOptions serializerOptions)
        {
            using IDisposable _ =
                CodeTimingSessionManager.Current.Measure("JsonSerializer.Serialize", MeasurementSettings.ExcludeJsonSerializationInPercentages);

            return JsonSerializer.Serialize(value, serializerOptions);
        }
    }
}
