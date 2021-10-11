#nullable disable

using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when referencing a local ID that was assigned to a different resource type.
    /// </summary>
    [PublicAPI]
    public sealed class IncompatibleLocalIdTypeException : JsonApiException
    {
        public IncompatibleLocalIdTypeException(string localId, string declaredType, string currentType)
            : base(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = "Incompatible type in Local ID usage.",
                Detail = $"Local ID '{localId}' belongs to resource type '{declaredType}' instead of '{currentType}'."
            })
        {
        }
    }
}
