using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

internal sealed class ReadWriteFakers
{
    private readonly Lazy<Faker<WorkItem>> _lazyWorkItemFaker = new(() => new Faker<WorkItem>()
        .MakeDeterministic()
        .RuleFor(workItem => workItem.Description, faker => faker.Lorem.Sentence())
        .RuleFor(workItem => workItem.DueAt, faker => faker.Date.Future().TruncateToWholeMilliseconds())
        .RuleFor(workItem => workItem.Priority, faker => faker.PickRandom<WorkItemPriority>()));

    private readonly Lazy<Faker<WorkTag>> _lazyWorkTagFaker = new(() => new Faker<WorkTag>()
        .MakeDeterministic()
        .RuleFor(workTag => workTag.Text, faker => faker.Lorem.Word())
        .RuleFor(workTag => workTag.IsBuiltIn, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<UserAccount>> _lazyUserAccountFaker = new(() => new Faker<UserAccount>()
        .MakeDeterministic()
        .RuleFor(userAccount => userAccount.FirstName, faker => faker.Name.FirstName())
        .RuleFor(userAccount => userAccount.LastName, faker => faker.Name.LastName()));

    private readonly Lazy<Faker<WorkItemGroup>> _lazyWorkItemGroupFaker = new(() => new Faker<WorkItemGroup>()
        .MakeDeterministic()
        .RuleFor(group => group.Name, faker => faker.Lorem.Word())
        .RuleFor(group => group.IsPublic, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<RgbColor>> _lazyRgbColorFaker = new(() => new Faker<RgbColor>()
        .MakeDeterministic()
        .RuleFor(color => color.Id, faker => faker.Random.Hexadecimal(6))
        .RuleFor(color => color.DisplayName, faker => faker.Commerce.Color()));

    public Faker<WorkItem> WorkItem => _lazyWorkItemFaker.Value;
    public Faker<WorkTag> WorkTag => _lazyWorkTagFaker.Value;
    public Faker<UserAccount> UserAccount => _lazyUserAccountFaker.Value;
    public Faker<WorkItemGroup> WorkItemGroup => _lazyWorkItemGroupFaker.Value;
    public Faker<RgbColor> RgbColor => _lazyRgbColorFaker.Value;
}
