using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ToManyRelationshipRequestData<TResource> : ManyData<ResourceIdentifierObject<TResource>>
        where TResource : IIdentifiable
    {
    }
}
