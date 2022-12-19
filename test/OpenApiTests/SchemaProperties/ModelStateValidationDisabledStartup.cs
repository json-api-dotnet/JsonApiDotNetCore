using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;

namespace OpenApiTests.SchemaProperties;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ModelStateValidationDisabledStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.ValidateModelState = false;
    }
}

