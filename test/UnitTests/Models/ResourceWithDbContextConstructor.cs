using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.Data;

namespace UnitTests.Models
{
    public class ResourceWithDbContextConstructor : Identifiable
    {
        public AppDbContext AppDbContext { get; }

        public ResourceWithDbContextConstructor(AppDbContext appDbContext)
        {
            ArgumentGuard.NotNull(appDbContext, nameof(appDbContext));

            AppDbContext = appDbContext;
        }
    }
}