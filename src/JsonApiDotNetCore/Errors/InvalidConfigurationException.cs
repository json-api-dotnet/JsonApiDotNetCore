using System;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when configured usage of this library is invalid.
    /// </summary>
    public sealed class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
