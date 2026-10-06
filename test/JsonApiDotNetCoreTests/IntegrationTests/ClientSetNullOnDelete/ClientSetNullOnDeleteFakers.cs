using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

internal sealed class ClientSetNullOnDeleteFakers
{
    private readonly Lazy<Faker<Customer>> _lazyCustomerFaker = new(() =>
        new Faker<Customer>()
            .MakeDeterministic()
            .RuleFor(customer => customer.Name, faker => faker.Person.FullName));

    private readonly Lazy<Faker<Order>> _lazyOrderFaker = new(() =>
        new Faker<Order>()
            .MakeDeterministic()
            .RuleFor(order => order.Amount, faker => faker.Finance.Amount()));

    private readonly Lazy<Faker<ShoppingBasket>> _lazyShoppingBasketFaker = new(() =>
        new Faker<ShoppingBasket>()
            .MakeDeterministic()
            .RuleFor(shoppingBasket => shoppingBasket.ProductCount, faker => faker.Random.Int(1, 99)));

    private readonly Lazy<Faker<OrderAudit>> _lazyOrderAuditFaker = new(() =>
        new Faker<OrderAudit>()
            .MakeDeterministic()
            .RuleFor(audit => audit.Note, faker => faker.Lorem.Sentence()));

    private readonly Lazy<Faker<SupportTicket>> _lazySupportTicketFaker = new(() =>
        new Faker<SupportTicket>()
            .MakeDeterministic()
            .RuleFor(ticket => ticket.Description, faker => faker.Lorem.Sentence()));

    public Faker<Customer> Customer => _lazyCustomerFaker.Value;
    public Faker<Order> Order => _lazyOrderFaker.Value;
    public Faker<ShoppingBasket> ShoppingBasket => _lazyShoppingBasketFaker.Value;
    public Faker<OrderAudit> OrderAudit => _lazyOrderAuditFaker.Value;
    public Faker<SupportTicket> SupportTicket => _lazySupportTicketFaker.Value;
}
