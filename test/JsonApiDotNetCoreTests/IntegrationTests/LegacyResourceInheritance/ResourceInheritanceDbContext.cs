using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ResourceInheritanceDbContext : DbContext
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<Cat> Cats => Set<Cat>();
    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<Female> Females => Set<Female>();
    public DbSet<Male> Males => Set<Male>();
    public DbSet<Book> FictionBooks => Set<Book>();
    public DbSet<Video> NonFictionBooks => Set<Video>();

    public ResourceInheritanceDbContext(DbContextOptions<ResourceInheritanceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .ToTable("Persons")
            .HasDiscriminator<int>("Type")
            .HasValue<Male>(1)
            .HasValue<Female>(2);

        modelBuilder.Entity<Animal>()
            .ToTable("Animals")
            .HasDiscriminator<int>("Type")
            .HasValue<Cat>(1)
            .HasValue<Dog>(2);

        modelBuilder.Entity<Content>()
            .ToTable("Books")
            .HasDiscriminator<int>("Type")
            .HasValue<Video>(1)
            .HasValue<Book>(2);
    }
}
