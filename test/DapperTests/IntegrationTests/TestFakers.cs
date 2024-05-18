using Bogus;
using DapperExample.Models;
using TestBuildingBlocks;
using Person = DapperExample.Models.Person;
using RgbColorType = DapperExample.Models.RgbColor;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace DapperTests.IntegrationTests;

internal sealed class TestFakers
{
    private readonly Lazy<Faker<TodoItem>> _lazyTodoItemFaker = new(() =>
        new Faker<TodoItem>()
            .MakeDeterministic()
            .RuleFor(todoItem => todoItem.Description, faker => faker.Lorem.Sentence())
            .RuleFor(todoItem => todoItem.Priority, faker => faker.Random.Enum<TodoItemPriority>())
            .RuleFor(todoItem => todoItem.DurationInHours, faker => faker.Random.Long(1, 250))
            .RuleFor(todoItem => todoItem.CreatedAt, faker => faker.Date.PastOffset()
                .TruncateToWholeMilliseconds())
            .RuleFor(todoItem => todoItem.LastModifiedAt, faker => faker.Date.PastOffset()
                .TruncateToWholeMilliseconds()));

    private readonly Lazy<Faker<LoginAccount>> _lazyLoginAccountFaker = new(() =>
        new Faker<LoginAccount>()
            .MakeDeterministic()
            .RuleFor(loginAccount => loginAccount.UserName, faker => faker.Internet.UserName())
            .RuleFor(loginAccount => loginAccount.LastUsedAt, faker => faker.Date.PastOffset()
                .TruncateToWholeMilliseconds()));

    private readonly Lazy<Faker<AccountRecovery>> _lazyAccountRecoveryFaker = new(() =>
        new Faker<AccountRecovery>()
            .MakeDeterministic()
            .RuleFor(accountRecovery => accountRecovery.PhoneNumber, faker => faker.Person.Phone)
            .RuleFor(accountRecovery => accountRecovery.EmailAddress, faker => faker.Person.Email));

    private readonly Lazy<Faker<Person>> _lazyPersonFaker = new(() =>
        new Faker<Person>()
            .MakeDeterministic()
            .RuleFor(person => person.FirstName, faker => faker.Name.FirstName())
            .RuleFor(person => person.LastName, faker => faker.Name.LastName()));

    private readonly Lazy<Faker<Tag>> _lazyTagFaker = new(() =>
        new Faker<Tag>()
            .MakeDeterministic()
            .RuleFor(tag => tag.Name, faker => faker.Lorem.Word()));

    private readonly Lazy<Faker<RgbColorType>> _lazyRgbColorFaker = new(() =>
        new Faker<RgbColorType>()
            .MakeDeterministic()
            .RuleFor(rgbColor => rgbColor.Id, faker => RgbColorType.Create(faker.Random.Byte(), faker.Random.Byte(), faker.Random.Byte())
                .Id));

    public Faker<TodoItem> TodoItem => _lazyTodoItemFaker.Value;
    public Faker<Person> Person => _lazyPersonFaker.Value;
    public Faker<LoginAccount> LoginAccount => _lazyLoginAccountFaker.Value;
    public Faker<AccountRecovery> AccountRecovery => _lazyAccountRecoveryFaker.Value;
    public Faker<Tag> Tag => _lazyTagFaker.Value;
    public Faker<RgbColorType> RgbColor => _lazyRgbColorFaker.Value;
}
