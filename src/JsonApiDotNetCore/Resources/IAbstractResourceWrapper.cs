namespace JsonApiDotNetCore.Resources;

/// <summary>
/// Because an instance cannot be created from an abstract resource type, this wrapper is used to preserve that information.
/// </summary>
internal interface IAbstractResourceWrapper : IIdentifiable
{
    /// <summary>
    /// The abstract resource type.
    /// </summary>
    Type AbstractType { get; }
}
