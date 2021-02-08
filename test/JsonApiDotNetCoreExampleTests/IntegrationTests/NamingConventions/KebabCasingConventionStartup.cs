using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    public sealed class KebabCasingConventionStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public KebabCasingConventionStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.IncludeExceptionStackTraceInErrors = true;
            options.Namespace = "public-api";
            options.UseRelativeLinks = true;
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;

            var resolver = (DefaultContractResolver) options.SerializerSettings.ContractResolver;
            resolver!.NamingStrategy = new KebabCaseNamingStrategy();
        }
    }
}
