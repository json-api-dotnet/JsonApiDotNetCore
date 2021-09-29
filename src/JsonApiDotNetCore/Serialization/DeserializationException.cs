using System;
using JsonApiDotNetCore.Serialization.RequestAdapters;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// The error that is thrown when deserialization of a JSON:API request body fails.
    /// </summary>
    internal sealed class DeserializationException : Exception
    {
        public string GenericMessage { get; }
        public string SpecificMessage { get; }
        public string SourcePointer { get; }

        public DeserializationException(RequestAdapterPosition position, string genericMessage, string specificMessage)
            : base(genericMessage)
        {
            GenericMessage = genericMessage;
            SpecificMessage = specificMessage;
            SourcePointer = position?.ToSourcePointer();
        }
    }
}
