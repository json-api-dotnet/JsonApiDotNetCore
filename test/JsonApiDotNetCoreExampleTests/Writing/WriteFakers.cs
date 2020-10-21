using Bogus;

namespace JsonApiDotNetCoreExampleTests.Writing
{
    internal static class WriteFakers
    {
        public static Faker<WorkItem> WorkItem { get; } = new Faker<WorkItem>()
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.DueAt, f => f.Date.Future());

        public static Faker<UserAccount> UserAccount { get; } = new Faker<UserAccount>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName());
        
        public static Faker<WorkItemGroup> WorkItemGroup { get; } = new Faker<WorkItemGroup>()
            .RuleFor(p => p.Name, f => f.Lorem.Word());
        
        public static Faker<RgbColor> RgbColor { get; } = new Faker<RgbColor>()
            .RuleFor(p=>p.Id, f=>f.Random.Hexadecimal(6))
            .RuleFor(p => p.DisplayName, f => f.Lorem.Word());
    }
}
