using System;
using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging
{
    internal sealed class LoggingFakers : FakerContainer
    {
        private readonly Lazy<Faker<AuditEntry>> _lazyAuditEntryFaker = new(() =>
            new Faker<AuditEntry>()
                .UseSeed(GetFakerSeed())
                .RuleFor(auditEntry => auditEntry.UserName, faker => faker.Internet.UserName())
                .RuleFor(auditEntry => auditEntry.CreatedAt, faker => faker.Date.PastOffset()
                    .TruncateToWholeMilliseconds()));

        public Faker<AuditEntry> AuditEntry => _lazyAuditEntryFaker.Value;
    }
}
