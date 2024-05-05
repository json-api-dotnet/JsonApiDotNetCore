using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

internal sealed class ScopesFakers
{
    private readonly Lazy<Faker<Movie>> _lazyMovieFaker = new(() => new Faker<Movie>()
        .MakeDeterministic()
        .RuleFor(movie => movie.Title, faker => faker.Random.Words())
        .RuleFor(movie => movie.ReleaseYear, faker => faker.Random.Int(1900, 2050))
        .RuleFor(movie => movie.DurationInSeconds, faker => faker.Random.Int(300, 14400)));

    private readonly Lazy<Faker<Actor>> _lazyActorFaker = new(() => new Faker<Actor>()
        .MakeDeterministic()
        .RuleFor(actor => actor.Name, faker => faker.Person.FullName)
        .RuleFor(actor => actor.BornAt, faker => faker.Date.Past()));

    private readonly Lazy<Faker<Genre>> _lazyGenreFaker = new(() => new Faker<Genre>()
        .MakeDeterministic()
        .RuleFor(genre => genre.Name, faker => faker.Random.Word()));

    public Faker<Movie> Movie => _lazyMovieFaker.Value;
    public Faker<Actor> Actor => _lazyActorFaker.Value;
    public Faker<Genre> Genre => _lazyGenreFaker.Value;
}
