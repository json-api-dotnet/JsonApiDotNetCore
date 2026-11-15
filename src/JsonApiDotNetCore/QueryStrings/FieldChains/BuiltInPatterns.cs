using JetBrains.Annotations;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[PublicAPI]
public static class BuiltInPatterns
{
    public static FieldChainPattern SingleField { get; } = FieldChainPattern.Parse("F");
    public static FieldChainPattern ToOneChain { get; } = FieldChainPattern.Parse("O+");
    public static FieldChainPattern ToOneChainEndingInAttribute { get; } = FieldChainPattern.Parse("O*A");
    public static FieldChainPattern ToOneChainEndingInAttributeOrToOne { get; } = FieldChainPattern.Parse("O*[OA]");
    public static FieldChainPattern ToOneChainEndingInToMany { get; } = FieldChainPattern.Parse("O*M");
}
