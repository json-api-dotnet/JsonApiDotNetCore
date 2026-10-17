using JetBrains.Annotations;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[PublicAPI]
public static class BuiltInPatterns
{
    public static FieldChainPattern ToOneChain { get; } = FieldChainPattern.Parse("O+");
    public static FieldChainPattern ToOneOrCompoundChainEndingInScalarAttribute { get; } = FieldChainPattern.Parse("[OC]*S");
    public static FieldChainPattern ToOneOrCompoundChainEndingInToOneOrAttribute { get; } = FieldChainPattern.Parse("[OC]*[OA]");
    public static FieldChainPattern ToOneOrCompoundChainEndingInCollection { get; } = FieldChainPattern.Parse("[OC]*[MLQ]");
    public static FieldChainPattern CompoundOrCompoundCollectionChainEndingInField { get; } = FieldChainPattern.Parse("[CQ]*F");
}
