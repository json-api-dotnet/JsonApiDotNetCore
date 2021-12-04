using JetBrains.Annotations;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name
#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore;

/// <summary>
/// Represents a stripped-down copy of this type in the JsonApiDotNetCore project. It exists solely to fulfill the dependency needs for successfully
/// compiling the source-generated controllers in this project.
/// </summary>
[PublicAPI]
internal static class ArgumentGuard
{
    public static void NotNullNorEmpty(string? value, string name)
    {
    }
}
