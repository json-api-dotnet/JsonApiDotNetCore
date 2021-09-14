using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using OpenApiTests.Startups;

namespace OpenApiTests.ExistingOpenApiIntegration
{
    public sealed class ExistingOpenApiIntegrationStartup<TDbContext> : OpenApiStartup<TDbContext>
        where TDbContext : DbContext
    {
        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "api/v1";
            options.DefaultPageSize = new PageSize(10);
            options.MaximumPageSize = new PageSize(100);
            options.MaximumPageNumber = new PageNumber(50);
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;
            options.DefaultAttrCapabilities = AttrCapabilities.AllowView;

            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabCaseNamingStrategy()
            };
        }
    }
}
