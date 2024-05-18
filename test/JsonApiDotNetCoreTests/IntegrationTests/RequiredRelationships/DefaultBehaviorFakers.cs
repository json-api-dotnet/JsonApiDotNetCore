using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

internal sealed class DefaultBehaviorFakers
{
    private readonly Lazy<Faker<Order>> _lazyOrderFaker = new(() => new Faker<Order>()
        .MakeDeterministic()
        .RuleFor(order => order.Amount, faker => faker.Finance.Amount()));

    private readonly Lazy<Faker<Customer>> _lazyCustomerFaker = new(() => new Faker<Customer>()
        .MakeDeterministic()
        .RuleFor(customer => customer.EmailAddress, faker => faker.Person.Email));

    private readonly Lazy<Faker<Shipment>> _lazyShipmentFaker = new(() => new Faker<Shipment>()
        .MakeDeterministic()
        .RuleFor(shipment => shipment.TrackAndTraceCode, faker => faker.Commerce.Ean13())
        .RuleFor(shipment => shipment.ShippedAt, faker => faker.Date.Past().TruncateToWholeMilliseconds()));

    public Faker<Order> Order => _lazyOrderFaker.Value;
    public Faker<Customer> Customer => _lazyCustomerFaker.Value;
    public Faker<Shipment> Shipment => _lazyShipmentFaker.Value;
}
