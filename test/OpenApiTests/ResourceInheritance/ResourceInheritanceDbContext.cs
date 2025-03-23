using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using OpenApiTests.ResourceInheritance.Models;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ResourceInheritanceDbContext(DbContextOptions<ResourceInheritanceDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<District> Districts => Set<District>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<FamilyHome> FamilyHomes => Set<FamilyHome>();
    public DbSet<Mansion> Mansions => Set<Mansion>();

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Kitchen> Kitchens => Set<Kitchen>();
    public DbSet<Bedroom> Bedrooms => Set<Bedroom>();
    public DbSet<Bathroom> Bathrooms => Set<Bathroom>();
    public DbSet<LivingRoom> LivingRooms => Set<LivingRoom>();
    public DbSet<Toilet> Toilets => Set<Toilet>();

    public DbSet<Road> Roads => Set<Road>();
    public DbSet<CyclePath> CyclePaths => Set<CyclePath>();
}
