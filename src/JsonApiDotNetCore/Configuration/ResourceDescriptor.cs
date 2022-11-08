namespace JsonApiDotNetCore.Configuration;

internal sealed class ResourceDescriptor
{
    public Type ResourceClrType { get; }
    public Type IdClrType { get; }

    public ResourceDescriptor(Type resourceClrType, Type idClrType)
    {
        ArgumentGuard.NotNull(resourceClrType);
        ArgumentGuard.NotNull(idClrType);

        ResourceClrType = resourceClrType;
        IdClrType = idClrType;
    }
}
