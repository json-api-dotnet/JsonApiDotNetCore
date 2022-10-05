using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.Experiments;

internal sealed class ExperimentsFakers : FakerContainer
{
    private readonly Lazy<Faker<Customer>> _lazyCustomerFaker = new(() =>
        new Faker<Customer>()
            .UseSeed(GetFakerSeed())
            .RuleFor(customer => customer.Name, faker => faker.Person.FullName));

    private readonly Lazy<Faker<Order>> _lazyOrderFaker = new(() =>
        new Faker<Order>()
            .UseSeed(GetFakerSeed())
            .RuleFor(order => order.Amount, faker => faker.Finance.Amount()));

    private readonly Lazy<Faker<ShoppingBasket>> _lazyShoppingBasketFaker = new(() =>
        new Faker<ShoppingBasket>()
            .UseSeed(GetFakerSeed())
            .RuleFor(shoppingBasket => shoppingBasket.ProductCount, faker => faker.Random.Int(0, 5)));

    public Faker<Customer> Customer => _lazyCustomerFaker.Value;
    public Faker<Order> Order => _lazyOrderFaker.Value;
    public Faker<ShoppingBasket> ShoppingBasket => _lazyShoppingBasketFaker.Value;
}
