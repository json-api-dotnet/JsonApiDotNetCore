using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ToOneRelationshipRequestData<TResource> : SingleData<ResourceIdentifierObject<TResource>>
        where TResource : IIdentifiable
    {
    }
}
