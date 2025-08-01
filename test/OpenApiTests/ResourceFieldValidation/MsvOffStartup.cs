using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceFieldValidation;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MsvOffStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);

        options.ValidateModelState = false;
    }
}
