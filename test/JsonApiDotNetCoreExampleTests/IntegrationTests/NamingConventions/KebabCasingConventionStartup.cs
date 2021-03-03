using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class KebabCasingConventionStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "public-api";
            options.UseRelativeLinks = true;
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;

            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabCaseNamingStrategy()
            };
        }
    }
}
