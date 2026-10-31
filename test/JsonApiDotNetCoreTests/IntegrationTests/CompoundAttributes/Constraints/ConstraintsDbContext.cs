using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ConstraintsDbContext(DbContextOptions<ConstraintsDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Author>(b => b.ComplexProperty(author => author.Contact, cb => cb.ToJson()));
        builder.Entity<Book>(b => b.ComplexProperty(book => book.Details, cb => cb.ToJson()));

        base.OnModelCreating(builder);
    }
}
