using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

internal sealed class MultiTenancyFakers
{
    private readonly Lazy<Faker<WebShop>> _lazyWebShopFaker = new(() => new Faker<WebShop>()
        .MakeDeterministic()
        .RuleFor(webShop => webShop.Url, faker => faker.Internet.Url()));

    private readonly Lazy<Faker<WebProduct>> _lazyWebProductFaker = new(() => new Faker<WebProduct>()
        .MakeDeterministic()
        .RuleFor(webProduct => webProduct.Name, faker => faker.Commerce.ProductName())
        .RuleFor(webProduct => webProduct.Price, faker => faker.Finance.Amount()));

    public Faker<WebShop> WebShop => _lazyWebShopFaker.Value;
    public Faker<WebProduct> WebProduct => _lazyWebProductFaker.Value;
}
