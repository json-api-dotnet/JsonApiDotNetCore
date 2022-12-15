using System.Text.Json;

namespace JsonApiDotNetCore.Serialization.Request;

/// <summary>
/// A sentinel value that is temporarily stored in the attributes dictionary to postpone producing an error.
/// </summary>
internal sealed class JsonInvalidAttributeInfo
{
    public static readonly JsonInvalidAttributeInfo Id = new("id", typeof(string), "-", JsonValueKind.Undefined);

    public string AttributeName { get; }
    public Type AttributeType { get; }
    public string? JsonValue { get; }
    public JsonValueKind JsonType { get; }

    public JsonInvalidAttributeInfo(string attributeName, Type attributeType, string? jsonValue, JsonValueKind jsonType)
    {
        ArgumentGuard.NotNullNorEmpty(attributeName);
        ArgumentGuard.NotNull(attributeType);

        AttributeName = attributeName;
        AttributeType = attributeType;
        JsonValue = jsonValue;
        JsonType = jsonType;
    }
}
