using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OwnedAttributesDbContext(DbContextOptions<OwnedAttributesDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<AddressBook> AddressBooks => Set<AddressBook>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AddressBook>()
            .OwnsOne(addressBook => addressBook.EmergencyContact)
            .ToJson();

        builder.Entity<AddressBook>()
            .OwnsMany(addressBook => addressBook.Favorites)
            .ToJson();

        builder.Entity<Contact>()
            .OwnsOne(contact => contact.Content)
            .ToJson();

        base.OnModelCreating(builder);
    }
}
