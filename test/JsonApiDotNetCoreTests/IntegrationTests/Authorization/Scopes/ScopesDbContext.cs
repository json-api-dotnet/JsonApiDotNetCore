using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ScopesDbContext : TestableDbContext
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Genre> Genres => Set<Genre>();

    public ScopesDbContext(DbContextOptions<ScopesDbContext> options)
        : base(options)
    {
    }
}
