using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// The error that is thrown when (de)serialization of a JSON:API body fails.
    /// </summary>
    [PublicAPI]
    public class JsonApiSerializationException : Exception
    {
        public string GenericMessage { get; }
        public string SpecificMessage { get; }
        public int? AtomicOperationIndex { get; }

        public JsonApiSerializationException(string genericMessage, string specificMessage, Exception innerException = null, int? atomicOperationIndex = null)
            : base(genericMessage, innerException)
        {
            GenericMessage = genericMessage;
            SpecificMessage = specificMessage;
            AtomicOperationIndex = atomicOperationIndex;
        }
    }
}
