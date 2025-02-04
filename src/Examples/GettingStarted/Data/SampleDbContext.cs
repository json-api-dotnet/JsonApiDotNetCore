using GettingStarted.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class SampleDbContext(DbContextOptions<SampleDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<TinyHouse> TinyHouses => Set<TinyHouse>();
    public DbSet<BigHouse> BigHouses => Set<BigHouse>();
}
