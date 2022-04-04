namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Indicates how to handle derived types when resolving resource field chains.
/// </summary>
internal enum FieldChainInheritanceRequirement
{
    /// <summary>
    /// Do not consider derived types when resolving attributes or relationships.
    /// </summary>
    Disabled,

    /// <summary>
    /// Consider derived types when resolving attributes or relationships, but fail when multiple matches are found.
    /// </summary>
    RequireSingleMatch
}
