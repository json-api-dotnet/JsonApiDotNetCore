using System;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using TestBuildingBlocks;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests
{
    internal sealed class ExampleFakers : FakerContainer
    {
        private readonly Lazy<Faker<Author>> _lazyAuthorFaker = new Lazy<Faker<Author>>(() =>
            new Faker<Author>()
                .UseSeed(GetFakerSeed())
                .RuleFor(author => author.FirstName, f => f.Person.FirstName)
                .RuleFor(author => author.LastName, f => f.Person.LastName));

        private readonly Lazy<Faker<Article>> _lazyArticleFaker = new Lazy<Faker<Article>>(() =>
            new Faker<Article>()
                .UseSeed(GetFakerSeed())
                .RuleFor(article => article.Caption, f => f.Lorem.Word())
                .RuleFor(article => article.Url, f => f.Internet.Url()));

        private readonly Lazy<Faker<User>> _lazyUserFaker = new Lazy<Faker<User>>(() =>
            new Faker<User>()
                .UseSeed(GetFakerSeed())
                .RuleFor(user => user.UserName, f => f.Person.UserName)
                .RuleFor(user => user.Password, f => f.Internet.Password()));

        private readonly Lazy<Faker<TodoItem>> _lazyTodoItemFaker = new Lazy<Faker<TodoItem>>(() =>
            new Faker<TodoItem>()
                .UseSeed(GetFakerSeed())
                .RuleFor(todoItem => todoItem.Description, f => f.Random.Words()));

        private readonly Lazy<Faker<Person>> _lazyPersonFaker = new Lazy<Faker<Person>>(() =>
            new Faker<Person>()
                .UseSeed(GetFakerSeed())
                .RuleFor(person => person.FirstName, f => f.Person.FirstName)
                .RuleFor(person => person.LastName, f => f.Person.LastName));

        private readonly Lazy<Faker<Tag>> _lazyTagFaker = new Lazy<Faker<Tag>>(() =>
            new Faker<Tag>()
                .UseSeed(GetFakerSeed())
                .RuleFor(tag => tag.Name, f => f.Lorem.Word()));

        public Faker<Author> Author => _lazyAuthorFaker.Value;
        public Faker<Article> Article => _lazyArticleFaker.Value;
        public Faker<User> User => _lazyUserFaker.Value;
        public Faker<TodoItem> TodoItem => _lazyTodoItemFaker.Value;
        public Faker<Person> Person => _lazyPersonFaker.Value;
        public Faker<Tag> Tag => _lazyTagFaker.Value;
    }
}
