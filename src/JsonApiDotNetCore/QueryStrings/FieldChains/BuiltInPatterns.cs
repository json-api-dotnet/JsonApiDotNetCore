using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[PublicAPI]
public static class BuiltInPatterns
{
    public static FieldChainPattern SingleField { get; } = FieldChainPattern.Parse("F");
    public static FieldChainPattern ToOneChain { get; } = FieldChainPattern.Parse("O+");
    public static FieldChainPattern ToOneChainEndingInAttribute { get; } = FieldChainPattern.Parse("O*A");
    public static FieldChainPattern ToOneChainEndingInAttributeOrToOne { get; } = FieldChainPattern.Parse("O*[OA]");
    public static FieldChainPattern ToOneChainEndingInToMany { get; } = FieldChainPattern.Parse("O*M");
    public static FieldChainPattern RelationshipChainEndingInToMany { get; } = FieldChainPattern.Parse("R*M");
}
