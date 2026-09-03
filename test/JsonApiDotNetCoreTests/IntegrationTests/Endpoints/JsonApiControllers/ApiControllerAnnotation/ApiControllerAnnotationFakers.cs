using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation;

internal sealed class ApiControllerAnnotationFakers
{
    private readonly Lazy<Faker<LoginToken>> _lazyLoginTokenFaker = new(() => new Faker<LoginToken>()
        .MakeDeterministic()
        .RuleFor(loginToken => loginToken.Value, faker => faker.Internet.Password())
        .RuleFor(loginToken => loginToken.CreatedAt, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds()));

    public Faker<LoginToken> LoginToken => _lazyLoginTokenFaker.Value;
}
