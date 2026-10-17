using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Represents a filter scope on a resource or compound type container.
/// </summary>
[PublicAPI]
public sealed class AttributeContainerFilterScope : FilterScope
{
    /// <summary>
    /// Gets the container in scope.
    /// </summary>
    public IAttributeContainer Container { get; }

    public AttributeContainerFilterScope(IAttributeContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        Container = container;
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return Container.ToString();
    }
}
