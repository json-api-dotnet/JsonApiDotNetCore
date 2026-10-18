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

    public static FieldChainPattern ToOneOrCompoundChainEndingInAttribute { get; } = FieldChainPattern.Parse("[OC]*A");
    public static FieldChainPattern ToOneOrCompoundChainEndingInAttributeOrToOneOrCompound { get; } = FieldChainPattern.Parse("[OC]*[AOC]");
    public static FieldChainPattern ToOneOrCompoundChainEndingInCollection { get; } = FieldChainPattern.Parse("[OC]*[MLQ]");
    public static FieldChainPattern SparseFieldSetChain { get; } = FieldChainPattern.Parse("[CQ]*F");

    [Obsolete("""This is no longer used and will be removed in a future version. Instead, use: FieldChainPattern.Parse("R*M")""")]
    public static FieldChainPattern RelationshipChainEndingInToMany { get; } = FieldChainPattern.Parse("R*M");
}
