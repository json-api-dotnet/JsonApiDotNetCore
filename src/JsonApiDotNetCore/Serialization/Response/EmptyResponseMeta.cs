namespace JsonApiDotNetCore.Serialization.Response;

/// <inheritdoc cref="IResponseMeta" />
public sealed class EmptyResponseMeta : IResponseMeta
{
    /// <inheritdoc />
    public IDictionary<string, object?>? GetMeta()
    {
        return null;
    }
}
