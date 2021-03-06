using Bogus;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization
{
    public class SerializationTestsSetupBase
    {
        protected IResourceGraph ResourceGraph { get; }
        protected Faker<Food> FoodFaker { get; }
        protected Faker<Song> SongFaker { get; }
        protected Faker<Article> ArticleFaker { get; }
        protected Faker<Blog> BlogFaker { get; }
        protected Faker<Person> PersonFaker { get; }

        protected SerializationTestsSetupBase()
        {
            ResourceGraph = BuildGraph();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            ArticleFaker = new Faker<Article>()
                .RuleFor(article => article.Title, faker => faker.Hacker.Phrase())
                .RuleFor(article => article.Id, faker => faker.UniqueIndex + 1);

            PersonFaker = new Faker<Person>()
                .RuleFor(person => person.Name, faker => faker.Person.FullName)
                .RuleFor(person => person.Id, faker => faker.UniqueIndex + 1);

            BlogFaker = new Faker<Blog>()
                .RuleFor(blog => blog.Title, faker => faker.Hacker.Phrase())
                .RuleFor(blog => blog.Id, faker => faker.UniqueIndex + 1);

            SongFaker = new Faker<Song>()
                .RuleFor(song => song.Title, faker => faker.Lorem.Sentence())
                .RuleFor(song => song.Id, faker => faker.UniqueIndex + 1);

            FoodFaker = new Faker<Food>()
                .RuleFor(food => food.Dish, faker => faker.Lorem.Sentence())
                .RuleFor(food => food.Id, faker => faker.UniqueIndex + 1);

            // @formatter:wrap_chained_method_calls restore
            // @formatter:keep_existing_linebreaks restore
        }

        private IResourceGraph BuildGraph()
        {
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.Add<TestResource>("testResource");
            resourceGraphBuilder.Add<TestResourceWithList>("testResource-with-list");

            BuildOneToOneRelationships(resourceGraphBuilder);
            BuildOneToManyRelationships(resourceGraphBuilder);
            BuildCollectiveRelationships(resourceGraphBuilder);

            resourceGraphBuilder.Add<Article>();
            resourceGraphBuilder.Add<Person>();
            resourceGraphBuilder.Add<Blog>();
            resourceGraphBuilder.Add<Food>();
            resourceGraphBuilder.Add<Song>();

            resourceGraphBuilder.Add<TestResourceWithAbstractRelationship>();
            resourceGraphBuilder.Add<BaseModel>();
            resourceGraphBuilder.Add<FirstDerivedModel>();
            resourceGraphBuilder.Add<SecondDerivedModel>();

            return resourceGraphBuilder.Build();
        }

        private static void BuildOneToOneRelationships(ResourceGraphBuilder resourceGraphBuilder)
        {
            resourceGraphBuilder.Add<OneToOnePrincipal>("oneToOnePrincipals");
            resourceGraphBuilder.Add<OneToOneDependent>("oneToOneDependents");
            resourceGraphBuilder.Add<OneToOneRequiredDependent>("oneToOneRequiredDependents");
        }

        private static void BuildOneToManyRelationships(ResourceGraphBuilder resourceGraphBuilder)
        {
            resourceGraphBuilder.Add<OneToManyPrincipal>("oneToManyPrincipals");
            resourceGraphBuilder.Add<OneToManyDependent>("oneToManyDependents");
            resourceGraphBuilder.Add<OneToManyRequiredDependent>("oneToMany-requiredDependents");
        }

        private static void BuildCollectiveRelationships(ResourceGraphBuilder resourceGraphBuilder)
        {
            resourceGraphBuilder.Add<MultipleRelationshipsPrincipalPart>("multiPrincipals");
            resourceGraphBuilder.Add<MultipleRelationshipsDependentPart>("multiDependents");
        }
    }
}
