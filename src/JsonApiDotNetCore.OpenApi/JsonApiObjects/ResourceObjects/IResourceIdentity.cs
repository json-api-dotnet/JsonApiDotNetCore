using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal interface IResourceIdentity
{
    string Type { get; set; }
    string Id { get; set; }
}
