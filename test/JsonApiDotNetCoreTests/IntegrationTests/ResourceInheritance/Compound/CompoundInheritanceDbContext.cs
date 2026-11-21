using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract class CompoundInheritanceDbContext(DbContextOptions options)
    : TestableDbContext(options)
{
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<Apartment> Apartments => Set<Apartment>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<Parking> Parkings => Set<Parking>();
    public DbSet<ListingAgent> ListingAgents => Set<ListingAgent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Residence>()
            .ComplexProperty(residence => residence.Address, complexBuilder => complexBuilder.ToJson("Address"));

        builder.Entity<Residence>()
            .ComplexCollection(residence => residence.Features, complexBuilder => complexBuilder.ToJson("Features"));

        builder.Entity<Residence>()
            .ComplexCollection(residence => residence.Rooms, complexBuilder => complexBuilder.ToJson("Rooms"));

        builder.Entity<Apartment>()
            .ComplexProperty(apartment => apartment.Building, complexBuilder => complexBuilder.ToJson("Building"));

        builder.Entity<Apartment>()
            .ComplexProperty(apartment => apartment.Insurance, complexBuilder => complexBuilder.ToJson("ApartmentInsurance"));

        builder.Entity<House>()
            .ComplexProperty(house => house.Garden, complexBuilder => complexBuilder.ToJson("Garden"));

        builder.Entity<House>()
            .ComplexProperty(house => house.Insurance, complexBuilder => complexBuilder.ToJson("HouseInsurance"));

        builder.Entity<Parking>()
            .ComplexProperty(parking => parking.Dimensions, complexBuilder => complexBuilder.ToJson("Dimensions"));

        builder.Entity<Parking>()
            .ComplexProperty(parking => parking.Insurance, complexBuilder => complexBuilder.ToJson("ParkingInsurance"));

        builder.Entity<ListingAgent>()
            .HasMany(agent => agent.Listings)
            .WithOne(listing => listing.Agent);

        base.OnModelCreating(builder);
    }
}
