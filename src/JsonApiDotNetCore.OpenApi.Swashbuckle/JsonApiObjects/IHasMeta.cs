using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal interface IHasMeta
{
    Meta Meta { get; set; }
}
