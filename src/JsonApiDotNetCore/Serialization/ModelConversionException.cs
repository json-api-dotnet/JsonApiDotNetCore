using System;
using JsonApiDotNetCore.Serialization.RequestAdapters;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// The error that is thrown when unable to convert a deserialized request body to an ASP.NET model.
    /// </summary>
    internal sealed class ModelConversionException : Exception
    {
        public string GenericMessage { get; }
        public string SpecificMessage { get; }
        public string SourcePointer { get; }

        public ModelConversionException(RequestAdapterPosition position, string genericMessage, string specificMessage)
            : base(genericMessage)
        {
            ArgumentGuard.NotNull(position, nameof(position));

            GenericMessage = genericMessage;
            SpecificMessage = specificMessage;
            SourcePointer = position.ToSourcePointer();
        }
    }
}
