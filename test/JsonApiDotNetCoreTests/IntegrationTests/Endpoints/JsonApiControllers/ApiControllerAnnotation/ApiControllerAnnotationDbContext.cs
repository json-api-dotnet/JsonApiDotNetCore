using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ApiControllerAnnotationDbContext(DbContextOptions<ApiControllerAnnotationDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<LoginToken> LoginTokens => Set<LoginToken>();
}
