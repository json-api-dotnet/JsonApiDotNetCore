namespace JsonApiDotNetCore.Configuration;

internal sealed class ResourceDescriptor
{
    public Type ResourceClrType { get; }
    public Type IdClrType { get; }

    public ResourceDescriptor(Type resourceClrType, Type idClrType)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);
        ArgumentNullException.ThrowIfNull(idClrType);

        ResourceClrType = resourceClrType;
        IdClrType = idClrType;
    }
}
