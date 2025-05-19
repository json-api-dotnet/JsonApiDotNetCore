using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[PublicAPI]
public static class BuiltInPatterns
{
    // TODO: Requires changing pattern syntax:
    // - O = to-one
    // - A = non-collection attribute
    // - C = collection (attribute or to-many)
    // - M = (obsolete)

    public static FieldChainPattern SingleField { get; } = FieldChainPattern.Parse("F");
    public static FieldChainPattern ToOneChain { get; } = FieldChainPattern.Parse("O+");
    public static FieldChainPattern ToOneChainEndingInAttribute { get; } = FieldChainPattern.Parse("O*A+");
    public static FieldChainPattern ToOneChainEndingInAttributeOrToOne { get; } = FieldChainPattern.Parse("O*[OA]+"); // TODO: This isn't entirely correct.
    public static FieldChainPattern ToOneChainEndingInToMany { get; } = FieldChainPattern.Parse("O*[MA]+"); // TODO: This isn't entirely correct.

    [Obsolete("""This is no longer used and will be removed in a future version. Instead, use: FieldChainPattern.Parse("R*M")""")]
    public static FieldChainPattern RelationshipChainEndingInToMany { get; } = FieldChainPattern.Parse("R*[MA]+"); // TODO: This isn't entirely correct.
}
