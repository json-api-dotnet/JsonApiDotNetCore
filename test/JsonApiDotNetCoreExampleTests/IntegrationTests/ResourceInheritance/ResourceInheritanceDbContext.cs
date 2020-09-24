using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class ResourceInheritanceDbContext : DbContext
    {
        public ResourceInheritanceDbContext(DbContextOptions<ResourceInheritanceDbContext> options)
            : base(options) { }
        
        public DbSet<Person> People  { get; set; }
        
        public DbSet<Cat> Cats { get; set; }
        
        public DbSet<Dog> Dogs { get; set; }
        
        public DbSet<Female> Females { get; set; }
        
        public DbSet<Male> Males { get; set; }
        
        public DbSet<FictionBook> FictionBooks { get; set; }
        
        public DbSet<NonFictionBook> NonFictionBooks { get; set; }

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
            
            modelBuilder.Entity<Literature>()
                .ToTable("Books")
                .HasDiscriminator<int>("Type")
                .HasValue<NonFictionBook>(1)
                .HasValue<FictionBook>(2);
            
            modelBuilder.Entity<LiteraturePerson>()
                .HasKey(pp => new { pp.LiteratureId, pp.PersonId });
        }
    }
}
