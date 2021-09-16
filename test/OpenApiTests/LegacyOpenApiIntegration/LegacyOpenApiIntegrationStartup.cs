using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

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

            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabCaseNamingStrategy()
            };
        }
    }
}
