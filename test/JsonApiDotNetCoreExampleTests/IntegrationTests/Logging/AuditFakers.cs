using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    internal sealed class AuditFakers : FakerContainer
    {
        private readonly Lazy<Faker<AuditEntry>> _lazyAuditEntryFaker = new Lazy<Faker<AuditEntry>>(() =>
            new Faker<AuditEntry>()
                .UseSeed(GetFakerSeed())
                .RuleFor(auditEntry => auditEntry.UserName, f => f.Internet.UserName())
                .RuleFor(auditEntry => auditEntry.CreatedAt, f => f.Date.PastOffset()));

        public Faker<AuditEntry> AuditEntry => _lazyAuditEntryFaker.Value;
    }
}
