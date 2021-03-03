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
                .RuleFor(f => f.Title, f => f.Hacker.Phrase())
                .RuleFor(f => f.Id, f => f.UniqueIndex + 1);

            PersonFaker = new Faker<Person>()
                .RuleFor(f => f.Name, f => f.Person.FullName)
                .RuleFor(f => f.Id, f => f.UniqueIndex + 1);

            BlogFaker = new Faker<Blog>()
                .RuleFor(f => f.Title, f => f.Hacker.Phrase())
                .RuleFor(f => f.Id, f => f.UniqueIndex + 1);

            SongFaker = new Faker<Song>()
                .RuleFor(f => f.Title, f => f.Lorem.Sentence())
                .RuleFor(f => f.Id, f => f.UniqueIndex + 1);

            FoodFaker = new Faker<Food>()
                .RuleFor(f => f.Dish, f => f.Lorem.Sentence())
                .RuleFor(f => f.Id, f => f.UniqueIndex + 1);

            // @formatter:wrap_chained_method_calls restore
            // @formatter:keep_existing_linebreaks restore
        }

        private IResourceGraph BuildGraph()
        {
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.Add<TestResource>("testResource");
            resourceGraphBuilder.Add<TestResourceWithList>("testResource-with-list");
            // one to one relationships
            resourceGraphBuilder.Add<OneToOnePrincipal>("oneToOnePrincipals");
            resourceGraphBuilder.Add<OneToOneDependent>("oneToOneDependents");
            resourceGraphBuilder.Add<OneToOneRequiredDependent>("oneToOneRequiredDependents");
            // one to many relationships
            resourceGraphBuilder.Add<OneToManyPrincipal>("oneToManyPrincipals");
            resourceGraphBuilder.Add<OneToManyDependent>("oneToManyDependents");
            resourceGraphBuilder.Add<OneToManyRequiredDependent>("oneToMany-requiredDependents");
            // collective relationships
            resourceGraphBuilder.Add<MultipleRelationshipsPrincipalPart>("multiPrincipals");
            resourceGraphBuilder.Add<MultipleRelationshipsDependentPart>("multiDependents");

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
    }
}
