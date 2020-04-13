using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization
{
    public class SerializationTestsSetupBase
    {
        protected IResourceGraph _resourceGraph;
        protected readonly Faker<Food> _foodFaker;
        protected readonly Faker<Song> _songFaker;
        protected readonly Faker<Article> _articleFaker;
        protected readonly Faker<Blog> _blogFaker;
        protected readonly Faker<Person> _personFaker;

        public SerializationTestsSetupBase()
        {
            _resourceGraph = BuildGraph();
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

        protected IResourceGraph BuildGraph()
        {
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions());
            resourceGraphBuilder.AddResource<TestResource>("testResource");
            resourceGraphBuilder.AddResource<TestResourceWithList>("testResource-with-list");
            // one to one relationships
            resourceGraphBuilder.AddResource<OneToOnePrincipal>("oneToOnePrincipals");
            resourceGraphBuilder.AddResource<OneToOneDependent>("oneToOneDependents");
            resourceGraphBuilder.AddResource<OneToOneRequiredDependent>("oneToOneRequiredDependents");
            // one to many relationships
            resourceGraphBuilder.AddResource<OneToManyPrincipal>("oneToManyPrincipals");
            resourceGraphBuilder.AddResource<OneToManyDependent>("oneToManyDependents");
            resourceGraphBuilder.AddResource<OneToManyRequiredDependent>("oneToMany-requiredDependents");
            // collective relationships
            resourceGraphBuilder.AddResource<MultipleRelationshipsPrincipalPart>("multiPrincipals");
            resourceGraphBuilder.AddResource<MultipleRelationshipsDependentPart>("multiDependents");

            resourceGraphBuilder.AddResource<Article>();
            resourceGraphBuilder.AddResource<Person>();
            resourceGraphBuilder.AddResource<Blog>();
            resourceGraphBuilder.AddResource<Food>();
            resourceGraphBuilder.AddResource<Song>();

            return resourceGraphBuilder.Build();
        }
   }
}
