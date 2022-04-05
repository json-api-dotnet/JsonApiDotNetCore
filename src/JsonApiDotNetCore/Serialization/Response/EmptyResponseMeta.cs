namespace JsonApiDotNetCore.Serialization.Response;

/// <inheritdoc />
public sealed class EmptyResponseMeta : IResponseMeta
{
    /// <inheritdoc />
    public IDictionary<string, object?>? GetMeta()
    {
        return null;
    }
}
