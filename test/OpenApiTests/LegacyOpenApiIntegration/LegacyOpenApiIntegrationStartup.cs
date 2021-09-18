using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class LegacyOpenApiIntegrationStartup<TDbContext> : OpenApiStartup<TDbContext>
        where TDbContext : DbContext
    {
        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "api/v1";
            options.DefaultAttrCapabilities = AttrCapabilities.AllowView;
            options.SerializerOptions.PropertyNamingPolicy = JsonKebabCaseNamingPolicy.Instance;
            options.SerializerOptions.DictionaryKeyPolicy = JsonKebabCaseNamingPolicy.Instance;
        }
    }
}
