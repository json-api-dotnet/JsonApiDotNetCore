using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JsonApiDotNetCoreExampleTests.Startups
{
    public sealed class AbsoluteLinksInApiNamespaceStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public AbsoluteLinksInApiNamespaceStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "api";
            options.UseRelativeLinks = false;
        }
    }
}
