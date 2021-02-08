using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceConstructorInjection
{
    public sealed class InjectionDbContext : DbContext
    {
        public ISystemClock SystemClock { get; }

        public DbSet<PostOffice> PostOffice { get; set; }
        public DbSet<GiftCertificate> GiftCertificates { get; set; }

        public InjectionDbContext(DbContextOptions<InjectionDbContext> options, ISystemClock systemClock)
            : base(options)
        {
            SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }
    }
}
