using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests;

/// <summary>
/// Lists the various extensibility points on <see cref="IResourceDefinition{TResource,TId}" />.
/// </summary>
[Flags]
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public enum ResourceDefinitionExtensibilityPoints
{
    None = 0,
    OnApplyIncludes = 1,
    OnApplyFilter = 1 << 1,
    OnApplySort = 1 << 2,
    OnApplyPagination = 1 << 3,
    OnApplySparseFieldSet = 1 << 4,
    OnRegisterQueryableHandlersForQueryStringParameters = 1 << 5,
    GetMeta = 1 << 6,
    OnPrepareWriteAsync = 1 << 7,
    OnSetToOneRelationshipAsync = 1 << 8,
    OnSetToManyRelationshipAsync = 1 << 9,
    OnAddToRelationshipAsync = 1 << 10,
    OnRemoveFromRelationshipAsync = 1 << 11,
    OnWritingAsync = 1 << 12,
    OnWriteSucceededAsync = 1 << 13,
    OnDeserialize = 1 << 14,
    OnSerialize = 1 << 15,

    Reading = OnApplyIncludes | OnApplyFilter | OnApplySort | OnApplyPagination | OnApplySparseFieldSet |
        OnRegisterQueryableHandlersForQueryStringParameters | GetMeta,

    Writing = OnPrepareWriteAsync | OnSetToOneRelationshipAsync | OnSetToManyRelationshipAsync | OnAddToRelationshipAsync | OnRemoveFromRelationshipAsync |
        OnWritingAsync | OnWriteSucceededAsync,

    Serialization = OnDeserialize | OnSerialize,

    All = Reading | Writing | Serialization
}
