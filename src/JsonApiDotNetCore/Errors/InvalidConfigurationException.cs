using JetBrains.Annotations;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when configured usage of this library is invalid.
/// </summary>
[PublicAPI]
public sealed class InvalidConfigurationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
