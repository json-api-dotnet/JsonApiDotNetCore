using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization
{
    public class SerializationTestsSetupBase
    {
        protected IResourceGraphExplorer _graph;
        protected readonly Faker<Food> _foodFaker;
        protected readonly Faker<Song> _songFaker;
        protected readonly Faker<Article> _articleFaker;
        protected readonly Faker<Blog> _blogFaker;
        protected readonly Faker<Person> _personFaker;

        public SerializationTestsSetupBase()
        {
            _graph = BuildGraph();
            _articleFaker = new Faker<Article>()
                    .RuleFor(f => f.Title, f => f.Hacker.Phrase())
                    .RuleFor(f => f.Id, f => f.UniqueIndex + 1);
            _personFaker = new Faker<Person>()
                    .RuleFor(f => f.Name, f => f.Person.FullName)
                    .RuleFor(f => f.Id, f => f.UniqueIndex + 1);
            _blogFaker = new Faker<Blog>()
                    .RuleFor(f => f.Title, f => f.Hacker.Phrase())
                    .RuleFor(f => f.Id, f => f.UniqueIndex + 1);
            _songFaker = new Faker<Song>()
                    .RuleFor(f => f.Title, f => f.Lorem.Sentence())
                    .RuleFor(f => f.Id, f => f.UniqueIndex + 1);
            _foodFaker = new Faker<Food>()
                    .RuleFor(f => f.Dish, f => f.Lorem.Sentence())
                    .RuleFor(f => f.Id, f => f.UniqueIndex + 1);
        }

        protected IResourceGraphExplorer BuildGraph()
        {
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");
            resourceGraphBuilder.AddResource<TestResourceWithList>("test-resource-with-list");
            // one to one relationships
            resourceGraphBuilder.AddResource<OneToOnePrincipal>("one-to-one-principals");
            resourceGraphBuilder.AddResource<OneToOneDependent>("one-to-one-dependents");
            resourceGraphBuilder.AddResource<OneToOneRequiredDependent>("one-to-one-required-dependents");
            // one to many relationships
            resourceGraphBuilder.AddResource<OneToManyPrincipal>("one-to-many-principals");
            resourceGraphBuilder.AddResource<OneToManyDependent>("one-to-many-dependents");
            resourceGraphBuilder.AddResource<OneToManyRequiredDependent>("one-to-many-required-dependents");
            // collective relationships
            resourceGraphBuilder.AddResource<MultipleRelationshipsPrincipalPart>("multi-principals");
            resourceGraphBuilder.AddResource<MultipleRelationshipsDependentPart>("multi-dependents");

            resourceGraphBuilder.AddResource<Article>();
            resourceGraphBuilder.AddResource<Person>();
            resourceGraphBuilder.AddResource<Blog>();
            resourceGraphBuilder.AddResource<Food>();
            resourceGraphBuilder.AddResource<Song>();

            return resourceGraphBuilder.Build();
        }
   }
}