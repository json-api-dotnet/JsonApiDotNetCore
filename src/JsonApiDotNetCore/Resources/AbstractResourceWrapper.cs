namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="IAbstractResourceWrapper" />
internal sealed class AbstractResourceWrapper<TId> : Identifiable<TId>, IAbstractResourceWrapper
{
    /// <inheritdoc />
    public Type AbstractType { get; }

    public AbstractResourceWrapper(Type abstractType)
    {
        ArgumentGuard.NotNull(abstractType);

        AbstractType = abstractType;
    }
}
