using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal interface IResourceIdentity : IHasMeta
{
    string Type { get; set; }
    string Id { get; set; }
}
