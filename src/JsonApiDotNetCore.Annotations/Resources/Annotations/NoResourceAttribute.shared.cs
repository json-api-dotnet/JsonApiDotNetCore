using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// When put on an Entity Framework Core entity, indicates that the type should not be added to the resource graph. This effectively suppresses the
/// warning at startup that this type does not implement <see cref="IIdentifiable" /> .
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class NoResourceAttribute : Attribute
{
}
