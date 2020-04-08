using System;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Exceptions
{
    public class JsonApiException : Exception
    {
        private static readonly JsonSerializerSettings _errorSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public Error Error { get; }

        public JsonApiException(Error error)
            : this(error, null)
        {
        }

        public JsonApiException(Error error, Exception innerException)
            : base(error.Title, innerException)
        {
            Error = error;
        }

        public override string Message => "Error = " + JsonConvert.SerializeObject(Error, _errorSerializerSettings);
    }
}
