using System.Collections.Generic;
using Bogus;
using JsonApiDotNetCoreExample.Models;

namespace OperationsExampleTests.Factories
{
    public static class ArticleFactory
    {
        public static Article Get()
        {
            var faker = new Faker<Article>();
            faker.RuleFor(m => m.Caption, f => f.Lorem.Sentence());
            return faker.Generate();
        }

        public static List<Article> Get(int count)
        {
            var articles = new List<Article>();
            for (int i = 0; i < count; i++)
                articles.Add(Get());

            return articles;
        }
    }
}
