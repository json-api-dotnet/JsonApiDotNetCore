using System.Collections.Generic;
using Bogus;
using JsonApiDotNetCoreExample.Models;

namespace OperationsExampleTests.Factories
{
    public static class AuthorFactory
    {
        public static Author Get()
        {
            var faker = new Faker<Author>();
            faker.RuleFor(m => m.Name, f => f.Person.UserName);
            return faker.Generate();
        }

        public static List<Author> Get(int count)
        {
            var authors = new List<Author>();
            for (int i = 0; i < count; i++)
                authors.Add(Get());

            return authors;
        }
    }
}
