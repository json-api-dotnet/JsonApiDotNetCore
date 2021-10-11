#nullable disable

using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests
{
    /// <summary>
    /// This is used solely in our tests, so we can assert which calls were made.
    /// </summary>
    public sealed class ResourceDefinitionHitCounter
    {
        internal IList<(Type, ExtensibilityPoint)> HitExtensibilityPoints { get; } = new List<(Type, ExtensibilityPoint)>();

        internal void TrackInvocation<TResource>(ExtensibilityPoint extensibilityPoint)
            where TResource : IIdentifiable
        {
            HitExtensibilityPoints.Add((typeof(TResource), extensibilityPoint));
        }

        internal void Reset()
        {
            HitExtensibilityPoints.Clear();
        }

        internal enum ExtensibilityPoint
        {
            OnApplyIncludes,
            OnApplyFilter,
            OnApplySort,
            OnApplyPagination,
            OnApplySparseFieldSet,
            OnRegisterQueryableHandlersForQueryStringParameters,
            GetMeta,
            OnPrepareWriteAsync,
            OnSetToOneRelationshipAsync,
            OnSetToManyRelationshipAsync,
            OnAddToRelationshipAsync,
            OnRemoveFromRelationshipAsync,
            OnWritingAsync,
            OnWriteSucceededAsync,
            OnDeserialize,
            OnSerialize
        }
    }
}
