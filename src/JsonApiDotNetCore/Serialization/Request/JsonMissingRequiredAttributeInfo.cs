namespace JsonApiDotNetCore.Serialization.Request;

/// <summary>
/// A sentinel value that is temporarily stored in the attributes dictionary to postpone producing an error.
/// </summary>
internal sealed class JsonMissingRequiredAttributeInfo
{
    public string AttributeName { get; }
    public string ResourceName { get; }

    public JsonMissingRequiredAttributeInfo(string attributeName, string resourceName)
    {
        ArgumentGuard.NotNull(attributeName);
        ArgumentGuard.NotNull(resourceName);

        AttributeName = attributeName;
        ResourceName = resourceName;
    }
}
