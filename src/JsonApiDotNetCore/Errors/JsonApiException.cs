using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The base class for an <see cref="Exception"/> that represents one or more json:api error objects in an unsuccessful response.
    /// </summary>
    public class JsonApiException : Exception
    {
        private static readonly JsonSerializerSettings _errorSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public IReadOnlyList<Error> Errors { get; }

        public JsonApiException(Error error, Exception innerException = null)
            : base(null, innerException)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));

            Errors = new[] {error};
        }

        public JsonApiException(IEnumerable<Error> errors, Exception innerException = null)
            : base(null, innerException)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));

            Errors = errors.ToList();

            if (!Errors.Any())
            {
                throw new ArgumentException("At least one error is required.", nameof(errors));
            }
        }

        public override string Message => "Errors = " + JsonConvert.SerializeObject(Errors, _errorSerializerSettings);
    }
}
