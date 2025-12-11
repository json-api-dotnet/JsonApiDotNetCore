using System.Text.Json;

namespace JsonApiDotNetCore.Serialization.Request;

/// <summary>
/// A sentinel value that is temporarily stored in the attributes dictionary to postpone producing an error.
/// </summary>
internal sealed class JsonInvalidAttributeInfo
{
    public static readonly JsonInvalidAttributeInfo Id = new("id", typeof(string), "-", JsonValueKind.Undefined, null);

    public string AttributeName { get; }
    public Type AttributeType { get; }
    public string? JsonValue { get; }
    public JsonValueKind JsonType { get; }
    public Exception? InnerException { get; }

    public JsonInvalidAttributeInfo(string attributeName, Type attributeType, string? jsonValue, JsonValueKind jsonType, Exception? innerException)
    {
        ArgumentNullException.ThrowIfNull(attributeName);
        ArgumentNullException.ThrowIfNull(attributeType);

        AttributeName = attributeName;
        AttributeType = attributeType;
        JsonValue = jsonValue;
        JsonType = jsonType;
        InnerException = innerException;
    }
}
