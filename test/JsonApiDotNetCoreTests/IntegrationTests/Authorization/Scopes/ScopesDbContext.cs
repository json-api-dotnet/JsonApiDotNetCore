using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ScopesDbContext(DbContextOptions<ScopesDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Movie>()
            .ComplexProperty(movie => movie.Production, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
