using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Bogus;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing
{
    internal class WriteFakers
    {
        private readonly Lazy<Faker<WorkItem>> _lazyWorkItemFaker = new Lazy<Faker<WorkItem>>(() =>
            new Faker<WorkItem>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workItem => workItem.Description, f => f.Lorem.Sentence())
                .RuleFor(workItem => workItem.DueAt, f => f.Date.Future())
                .RuleFor(workItem => workItem.Priority, f => f.PickRandom<WorkItemPriority>()));

        private readonly Lazy<Faker<WorkTag>> _lazyWorkTagsFaker = new Lazy<Faker<WorkTag>>(() =>
            new Faker<WorkTag>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workTag => workTag.Text, f => f.Lorem.Word())
                .RuleFor(workTag => workTag.IsBuiltIn, f => f.Random.Bool()));

        private readonly Lazy<Faker<UserAccount>> _lazyUserAccountFaker = new Lazy<Faker<UserAccount>>(() =>
            new Faker<UserAccount>()
                .UseSeed(GetFakerSeed())
                .RuleFor(userAccount => userAccount.FirstName, f => f.Name.FirstName())
                .RuleFor(userAccount => userAccount.LastName, f => f.Name.LastName()));

        private readonly Lazy<Faker<WorkItemGroup>> _lazyWorkItemGroupFaker = new Lazy<Faker<WorkItemGroup>>(() =>
            new Faker<WorkItemGroup>()
                .UseSeed(GetFakerSeed())
                .RuleFor(group => group.Name, f => f.Lorem.Word())
                .RuleFor(group => group.IsPublic, f => f.Random.Bool()));

        private readonly Lazy<Faker<RgbColor>> _lazyRgbColorFaker = new Lazy<Faker<RgbColor>>(() =>
            new Faker<RgbColor>()
                .UseSeed(GetFakerSeed())
                .RuleFor(color => color.Id, f => f.Random.Hexadecimal(6))
                .RuleFor(color => color.DisplayName, f => f.Lorem.Word()));

        public Faker<WorkItem> WorkItem => _lazyWorkItemFaker.Value;
        public Faker<WorkTag> WorkTags => _lazyWorkTagsFaker.Value;
        public Faker<UserAccount> UserAccount => _lazyUserAccountFaker.Value;
        public Faker<WorkItemGroup> WorkItemGroup => _lazyWorkItemGroupFaker.Value;
        public Faker<RgbColor> RgbColor => _lazyRgbColorFaker.Value;

        private static int GetFakerSeed()
        {
            // The goal here is to have stable data over multiple test runs, but at the same time different data per test case.

            MethodBase testMethod = GetTestMethod();
            var testName = testMethod.DeclaringType?.FullName + "." + testMethod.Name;

            return GetDeterministicHashCode(testName);
        }

        private static MethodBase GetTestMethod()
        {
            var stackTrace = new StackTrace();

            var testMethod = stackTrace.GetFrames()
                .Select(stackFrame => stackFrame?.GetMethod())
                .FirstOrDefault(IsTestMethod);

            if (testMethod == null)
            {
                // If called after the first await statement, the test method is no longer on the stack,
                // but has been replaced with the compiler-generated async/wait state machine.
                throw new InvalidOperationException("Fakers can only be used from within (the start of) a test method.");
            }

            return testMethod;
        }

        private static bool IsTestMethod(MethodBase method)
        {
            if (method == null)
            {
                return false;
            }

            return method.GetCustomAttribute(typeof(FactAttribute)) != null || method.GetCustomAttribute(typeof(TheoryAttribute)) != null;
        }

        private static int GetDeterministicHashCode(string source)
        {
            // https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < source.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ source[i];

                    if (i == source.Length - 1)
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ source[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }
    }
}
