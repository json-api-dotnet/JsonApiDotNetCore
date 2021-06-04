using System;
using Bogus;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models;
using TestBuildingBlocks;
using Person = JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models.Person;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks
{
    internal sealed class HooksFakers : FakerContainer
    {
        private readonly Lazy<Faker<Author>> _lazyAuthorFaker = new Lazy<Faker<Author>>(() =>
            new Faker<Author>()
                .UseSeed(GetFakerSeed())
                .RuleFor(author => author.Name, faker => faker.Person.FullName));

        private readonly Lazy<Faker<Article>> _lazyArticleFaker = new Lazy<Faker<Article>>(() =>
            new Faker<Article>()
                .UseSeed(GetFakerSeed())
                .RuleFor(article => article.Caption, faker => faker.Lorem.Word())
                .RuleFor(article => article.Url, faker => faker.Internet.Url()));

        private readonly Lazy<Faker<User>> _lazyUserFaker = new Lazy<Faker<User>>(() =>
            new Faker<User>()
                .UseSeed(GetFakerSeed())
                .RuleFor(user => user.UserName, faker => faker.Person.UserName)
                .RuleFor(user => user.Password, faker => faker.Internet.Password()));

        private readonly Lazy<Faker<TodoItem>> _lazyTodoItemFaker = new Lazy<Faker<TodoItem>>(() =>
            new Faker<TodoItem>()
                .UseSeed(GetFakerSeed())
                .RuleFor(todoItem => todoItem.Description, faker => faker.Random.Words()));

        private readonly Lazy<Faker<Person>> _lazyPersonFaker = new Lazy<Faker<Person>>(() =>
            new Faker<Person>()
                .UseSeed(GetFakerSeed())
                .RuleFor(person => person.Name, faker => faker.Person.FullName));

        private readonly Lazy<Faker<Tag>> _lazyTagFaker = new Lazy<Faker<Tag>>(() =>
            new Faker<Tag>()
                .UseSeed(GetFakerSeed())
                .RuleFor(tag => tag.Name, faker => faker.Lorem.Word()));

        public Faker<Author> Author => _lazyAuthorFaker.Value;
        public Faker<Article> Article => _lazyArticleFaker.Value;
        public Faker<User> User => _lazyUserFaker.Value;
        public Faker<TodoItem> TodoItem => _lazyTodoItemFaker.Value;
        public Faker<Person> Person => _lazyPersonFaker.Value;
        public Faker<Tag> Tag => _lazyTagFaker.Value;
    }
}
