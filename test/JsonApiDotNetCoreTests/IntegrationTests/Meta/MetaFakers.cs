using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

internal sealed class MetaFakers
{
    private readonly Lazy<Faker<ProductFamily>> _lazyProductFamilyFaker = new(() => new Faker<ProductFamily>()
        .MakeDeterministic()
        .RuleFor(productFamily => productFamily.Name, faker => faker.Commerce.ProductName()));

    private readonly Lazy<Faker<SupportTicket>> _lazySupportTicketFaker = new(() => new Faker<SupportTicket>()
        .MakeDeterministic()
        .RuleFor(supportTicket => supportTicket.Description, faker => faker.Lorem.Paragraph()));

    public Faker<ProductFamily> ProductFamily => _lazyProductFamilyFaker.Value;
    public Faker<SupportTicket> SupportTicket => _lazySupportTicketFaker.Value;
}
