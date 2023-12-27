using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ScopesDbContext(DbContextOptions<ScopesDbContext> options) : TestableDbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Genre> Genres => Set<Genre>();
}
