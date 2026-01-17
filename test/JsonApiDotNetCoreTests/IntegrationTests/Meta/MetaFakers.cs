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

    private readonly Lazy<Faker<Dictionary<string, object?>>> _lazyDocumentMetaFaker = new(() => new Faker<Dictionary<string, object?>>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Dictionary<string, object?>
        {
            ["requestId"] = faker.Random.Guid().ToString(),
            ["isUrgent"] = faker.Random.Bool()
        }));

    private readonly Lazy<Faker<Dictionary<string, object?>>> _lazyResourceMetaFaker = new(() => new Faker<Dictionary<string, object?>>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Dictionary<string, object?>
        {
            ["editedBy"] = faker.Internet.UserName(),
            ["revision"] = faker.Random.Int(1, 10)
        }));

    private readonly Lazy<Faker<Dictionary<string, object?>>> _lazyRelationshipMetaFaker = new(() => new Faker<Dictionary<string, object?>>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Dictionary<string, object?>
        {
            ["source"] = faker.PickRandom("ui", "api", "import"),
            ["confidence"] = faker.Random.Double(0.1),
            ["details"] = null
        }));

    private readonly Lazy<Faker<Dictionary<string, object?>>> _lazyIdentifierMetaFaker = new(() => new Faker<Dictionary<string, object?>>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Dictionary<string, object?>
        {
            ["index"] = faker.IndexFaker,
            ["optionalNote"] = faker.Lorem.Word()
        }));

    private readonly Lazy<Faker<Dictionary<string, object?>>> _lazyOperationMetaFaker = new(() => new Faker<Dictionary<string, object?>>()
        .MakeDeterministic()
        .CustomInstantiator(faker => new Dictionary<string, object?>
        {
            ["version"] = faker.Random.Int(1, 10)
        }));

    public Faker<ProductFamily> ProductFamily => _lazyProductFamilyFaker.Value;
    public Faker<SupportTicket> SupportTicket => _lazySupportTicketFaker.Value;
    public Faker<Dictionary<string, object?>> DocumentMeta => _lazyDocumentMetaFaker.Value;
    public Faker<Dictionary<string, object?>> ResourceMeta => _lazyResourceMetaFaker.Value;
    public Faker<Dictionary<string, object?>> RelationshipMeta => _lazyRelationshipMetaFaker.Value;
    public Faker<Dictionary<string, object?>> IdentifierMeta => _lazyIdentifierMetaFaker.Value;
    public Faker<Dictionary<string, object?>> OperationMeta => _lazyOperationMetaFaker.Value;
}
