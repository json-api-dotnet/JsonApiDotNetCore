using System.Text.Json.Serialization;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization;

// Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
partial class JsonApiSerializationContext
{
}

/// <summary>
/// Provides compile-time metadata about the set of JSON:API types used in JSON serialization of request/response bodies.
/// </summary>
[JsonSerializable(typeof(Document))]
public sealed partial class JsonApiSerializationContext : JsonSerializerContext
{
}
