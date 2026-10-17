using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompoundAttributesDbContext(DbContextOptions<CompoundAttributesDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<AddressBook> AddressBooks => Set<AddressBook>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactNote> Notes => Set<ContactNote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AddressBook>(b =>
        {
            b.ComplexProperty(a => a.EmergencyContact, cb => cb.ToJson());
            b.ComplexCollection(a => a.Favorites, cb => cb.ToJson());
            b.HasMany(a => a.Contacts).WithOne(c => c.AddressBook);
        });

        builder.Entity<Contact>(b =>
        {
            b.ComplexProperty(c => c.Content, cb => cb.ToJson());
            b.HasMany(c => c.Notes).WithOne();
        });

        base.OnModelCreating(builder);
    }
}
