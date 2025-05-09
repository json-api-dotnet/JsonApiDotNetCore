using GettingStarted.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace GettingStarted.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class SampleDbContext(DbContextOptions<SampleDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Entity<Person>()
            .OwnsOne(person => person.LivingAddress)
            .ToJson();

        builder.Entity<Person>()
            .OwnsOne(person => person.MailAddress)
            .ToJson();

        builder.Entity<Person>()
            .OwnsMany(person => person.Addresses)
            .ToJson();
    }
}
