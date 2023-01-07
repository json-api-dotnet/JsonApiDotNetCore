using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MsvOffStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.ValidateModelState = false;
    }
}
