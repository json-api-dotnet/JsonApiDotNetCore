using Bogus;
using JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models;
using Person = JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models.Person;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

internal sealed class ResponseSerializationFakers
{
    private const int FakerSeed = 0;
    private int _index;

    public Faker<Food> Food { get; }
    public Faker<Song> Song { get; }
    public Faker<Article> Article { get; }
    public Faker<Blog> Blog { get; }
    public Faker<Person> Person { get; }

    public ResponseSerializationFakers()
    {
        Article = new Faker<Article>()
            .UseSeed(FakerSeed)
            .RuleFor(article => article.Title, faker => faker.Hacker.Phrase())
            .RuleFor(article => article.Id, _ => ++_index);

        Person = new Faker<Person>()
            .UseSeed(FakerSeed)
            .RuleFor(person => person.Name, faker => faker.Person.FullName)
            .RuleFor(person => person.Id, _ => ++_index);

        Blog = new Faker<Blog>()
            .UseSeed(FakerSeed)
            .RuleFor(blog => blog.Title, faker => faker.Hacker.Phrase())
            .RuleFor(blog => blog.Id, _ => ++_index);

        Song = new Faker<Song>()
            .UseSeed(FakerSeed)
            .RuleFor(song => song.Title, faker => faker.Lorem.Sentence())
            .RuleFor(song => song.Id, _ => ++_index);

        Food = new Faker<Food>()
            .UseSeed(FakerSeed)
            .RuleFor(food => food.Dish, faker => faker.Lorem.Sentence())
            .RuleFor(food => food.Id, _ => ++_index);
    }
}
