using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when configured usage of this library is invalid.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
