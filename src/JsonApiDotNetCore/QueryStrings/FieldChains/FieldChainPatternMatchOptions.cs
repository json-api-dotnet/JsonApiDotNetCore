namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Indicates how to perform matching a pattern against a resource field chain.
/// </summary>
[Flags]
public enum FieldChainPatternMatchOptions
{
    /// <summary>
    /// Specifies that no options are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies to include fields on derived types in the search for a matching field.
    /// </summary>
    AllowDerivedTypes = 1
}
