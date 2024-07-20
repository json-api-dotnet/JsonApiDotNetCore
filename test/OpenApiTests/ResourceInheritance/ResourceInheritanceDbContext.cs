using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ResourceInheritanceDbContext(DbContextOptions<ResourceInheritanceDbContext> options) : TestableDbContext(options)
{
    public DbSet<District> Districts => Set<District>();

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<FamilyHome> FamilyHomes => Set<FamilyHome>();
    public DbSet<Mansion> Mansions => Set<Mansion>();
}
