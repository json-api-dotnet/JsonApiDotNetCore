using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Represents a filter scope on a resource or compound type container.
/// </summary>
[PublicAPI]
public sealed class AttributeContainerFilterParseScope : FilterParseScope
{
    /// <summary>
    /// Gets the container in scope.
    /// </summary>
    public IAttributeContainer Container { get; }

    public AttributeContainerFilterParseScope(IAttributeContainer container)
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
