using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JsonApiDotNetCoreExampleTests.Startups
{
    public sealed class AbsoluteLinksNoNamespaceStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public AbsoluteLinksNoNamespaceStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = null;
            options.UseRelativeLinks = false;
        }
    }
}
