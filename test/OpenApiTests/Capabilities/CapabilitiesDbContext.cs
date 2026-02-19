using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CapabilitiesDbContext(DbContextOptions<CapabilitiesDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Writer> Writers => Set<Writer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
}
