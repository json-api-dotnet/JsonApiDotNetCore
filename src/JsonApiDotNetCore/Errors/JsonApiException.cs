using System;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Errors
{
    public class JsonApiException : Exception
    {
        private static readonly JsonSerializerSettings _errorSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public Error Error { get; }

        public JsonApiException(Error error, Exception innerException = null)
            : base(error.Title, innerException)
        {
            Error = error;
        }

        public override string Message => "Error = " + JsonConvert.SerializeObject(Error, _errorSerializerSettings);
    }
}
