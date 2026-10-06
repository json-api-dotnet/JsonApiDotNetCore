using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph.ResourceGraphValidation;

internal static class TestDbContextFactory
{
    public static TDbContext Create<TDbContext>(Func<DbContextOptions<TDbContext>, TDbContext> construct,
        Action<DbContextOptionsBuilder<TDbContext>>? configure = null)
        where TDbContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        DefaultConfigureOptions(optionsBuilder);
        configure?.Invoke(optionsBuilder);

        return construct(optionsBuilder.Options);
    }

    public static void Add<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>(DefaultConfigureOptions);
    }

    private static void DefaultConfigureOptions(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
    }
}
