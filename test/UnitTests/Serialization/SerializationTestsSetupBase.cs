using System;
using System.Collections.Generic;
using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;

namespace UnitTests.Serialization
{
    public class SerializationTestsSetupBase
    {
        protected IResourceGraph _resourceGraph;
        protected IContextEntityProvider _provider;
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

        public class TestResource : Identifiable
        {
            [Attr] public string StringField { get; set; }
            [Attr] public DateTime DateTimeField { get; set; }
            [Attr] public DateTime? NullableDateTimeField { get; set; }
            [Attr] public int IntField { get; set; }
            [Attr] public int? NullableIntField { get; set; }
            [Attr] public Guid GuidField { get; set; }
            [Attr] public ComplexType ComplexField { get; set; }
            [Attr(isImmutable: true)] public string Immutable { get; set; }
        }

        public class TestResourceWithList : Identifiable
        {
            [Attr] public List<ComplexType> ComplexFields { get; set; }
        }

        public class ComplexType
        {
            public string CompoundName { get; set; }
        }

        public class OneToOnePrincipal : IdentifiableWithAttribute
        {
            [HasOne] public OneToOneDependent Dependent { get; set; }
        }

        public class OneToOneDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToOnePrincipal Principal { get; set; }
            public int? PrincipalId { get; set; }
        }

        public class OneToOneRequiredDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToOnePrincipal Principal { get; set; }
            public int PrincipalId { get; set; }
        }

        public class OneToManyDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToManyPrincipal Principal { get; set; }
            public int? PrincipalId { get; set; }
        }

        public class OneToManyRequiredDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToManyPrincipal Principal { get; set; }
            public int PrincipalId { get; set; }
        }

        public class OneToManyPrincipal : IdentifiableWithAttribute
        {
            [HasMany] public List<OneToManyDependent> Dependents { get; set; }
        }

        public class IdentifiableWithAttribute : Identifiable
        {
            [Attr] public string AttributeMember { get; set; }
        }

        public class MultipleRelationshipsPrincipalPart : IdentifiableWithAttribute
        {
            [HasOne] public OneToOneDependent PopulatedToOne { get; set; }
            [HasOne] public OneToOneDependent EmptyToOne { get; set; }
            [HasMany] public List<OneToManyDependent> PopulatedToManies { get; set; }
            [HasMany] public List<OneToManyDependent> EmptyToManies { get; set; }
            [HasOne] public MultipleRelationshipsPrincipalPart Multi { get; set; }
        }

        public class MultipleRelationshipsDependentPart : IdentifiableWithAttribute
        {
            [HasOne] public OneToOnePrincipal PopulatedToOne { get; set; }
            public int PopulatedToOneId { get; set; }
            [HasOne] public OneToOnePrincipal EmptyToOne { get; set; }
            public int? EmptyToOneId { get; set; }
            [HasOne] public OneToManyPrincipal PopulatedToMany { get; set; }
            public int PopulatedToManyId { get; set; }
            [HasOne] public OneToManyPrincipal EmptyToMany { get; set; }
            public int? EmptyToManyId { get; set; }
        }


        public class Article : Identifiable
        {
            [Attr] public string Title { get; set; }
            [HasOne] public Person Reviewer { get; set; }
            [HasOne] public Person Author { get; set; }
        }

        public class Person : Identifiable
        {
            [Attr] public string Name { get; set; }
            [HasMany] public List<Blog> Blogs { get; set; }
            [HasOne] public Food FavoriteFood { get; set; }
            [HasOne] public Song FavoriteSong { get; set; }
        }

        public class Blog : Identifiable
        {
            [Attr] public string Title { get; set; }
            [HasOne] public Person Reviewer { get; set; }
            [HasOne] public Person Author { get; set; }
        }

        public class Food : Identifiable
        {
            [Attr] public string Dish { get; set; }
        }

        public class Song : Identifiable
        {
            [Attr] public string Title { get; set; }
        }
    }
}