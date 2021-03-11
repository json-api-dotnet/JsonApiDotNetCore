using System.Collections.Generic;
using System.Linq;
using Bogus;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace UnitTests.ResourceHooks
{
    public class HooksDummyData
    {
        private readonly Faker<IdentifiableArticleTag> _identifiableArticleTagFaker;
        protected readonly Faker<TodoItem> TodoFaker;
        protected readonly Faker<Person> PersonFaker;
        protected readonly Faker<Article> ArticleFaker;
        protected readonly Faker<Tag> TagFaker;
        protected readonly Faker<ArticleTag> ArticleTagFaker;
        protected readonly Faker<Passport> PassportFaker;
        protected IResourceGraph ResourceGraph { get; }
        protected ResourceHook[] NoHooks { get; } = new ResourceHook[0];

        protected ResourceHook[] EnableDbValues { get; } =
        {
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeUpdateRelationship
        };

        protected ResourceHook[] DisableDbValues { get; } = new ResourceHook[0];

        protected HooksDummyData()
        {
            ResourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TodoItem>().Add<Person>().Add<Passport>()
                .Add<Article>().Add<IdentifiableArticleTag>().Add<Tag>().Build();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            TodoFaker = new Faker<TodoItem>()
                .RuleFor(todoItem => todoItem.Id, faker => faker.UniqueIndex + 1);

            PersonFaker = new Faker<Person>()
                .RuleFor(person => person.Id, faker => faker.UniqueIndex + 1);

            ArticleFaker = new Faker<Article>()
                .RuleFor(article => article.Id, faker => faker.UniqueIndex + 1);

            TagFaker = new Faker<Tag>()
                .RuleFor(tag => tag.Id, faker => faker.UniqueIndex + 1);

            ArticleTagFaker = new Faker<ArticleTag>();

            _identifiableArticleTagFaker = new Faker<IdentifiableArticleTag>()
                .RuleFor(identifiableArticleTag => identifiableArticleTag.Id, faker => faker.UniqueIndex + 1);

            PassportFaker = new Faker<Passport>()
                .RuleFor(passport => passport.Id, faker => faker.UniqueIndex + 1);

            // @formatter:wrap_chained_method_calls restore
            // @formatter:keep_existing_linebreaks restore
        }

        protected IList<TodoItem> CreateTodoWithToOnePerson()
        {
            TodoItem todoItem = TodoFaker.Generate();
            Person person = PersonFaker.Generate();
            List<TodoItem> todoList = todoItem.AsList();
            person.OneToOneTodoItem = todoItem;
            todoItem.OneToOnePerson = person;
            return todoList;
        }

        protected IEnumerable<TodoItem> CreateTodoWithOwner()
        {
            TodoItem todoItem = TodoFaker.Generate();
            Person person = PersonFaker.Generate();

            var todoSet = new HashSet<TodoItem>
            {
                todoItem
            };

            person.AssignedTodoItems = todoSet;
            todoItem.Owner = person;
            return todoSet;
        }

        protected (List<Article>, List<Tag>) CreateManyToManyData()
        {
            List<Tag> tagsSubset = TagFaker.Generate(3);
            List<ArticleTag> joinsSubSet = ArticleTagFaker.Generate(3);
            Article articleTagsSubset = ArticleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet.ToHashSet();

            for (int index = 0; index < 3; index++)
            {
                joinsSubSet[index].Article = articleTagsSubset;
                joinsSubSet[index].Tag = tagsSubset[index];
            }

            List<Tag> allTags = TagFaker.Generate(3).Concat(tagsSubset).ToList();
            List<ArticleTag> completeJoin = ArticleTagFaker.Generate(6);

            Article articleWithAllTags = ArticleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin.ToHashSet();

            for (int index = 0; index < 6; index++)
            {
                completeJoin[index].Article = articleWithAllTags;
                completeJoin[index].Tag = allTags[index];
            }

            List<Article> articles = ArrayFactory.Create(articleTagsSubset, articleWithAllTags).ToList();
            return (articles, allTags);
        }

        protected ManyToManyTestData CreateIdentifiableManyToManyData()
        {
            List<Tag> tagsSubset = TagFaker.Generate(3);
            List<IdentifiableArticleTag> joinsSubSet = _identifiableArticleTagFaker.Generate(3);
            Article articleTagsSubset = ArticleFaker.Generate();
            articleTagsSubset.IdentifiableArticleTags = joinsSubSet.ToHashSet();

            for (int index = 0; index < 3; index++)
            {
                joinsSubSet[index].Article = articleTagsSubset;
                joinsSubSet[index].Tag = tagsSubset[index];
            }

            List<Tag> allTags = TagFaker.Generate(3).Concat(tagsSubset).ToList();
            List<IdentifiableArticleTag> completeJoin = _identifiableArticleTagFaker.Generate(6);

            Article articleWithAllTags = ArticleFaker.Generate();
            articleWithAllTags.IdentifiableArticleTags = joinsSubSet.ToHashSet();

            for (int index = 0; index < 6; index++)
            {
                completeJoin[index].Article = articleWithAllTags;
                completeJoin[index].Tag = allTags[index];
            }

            List<IdentifiableArticleTag> allJoins = joinsSubSet.Concat(completeJoin).ToList();
            List<Article> articles = ArrayFactory.Create(articleTagsSubset, articleWithAllTags).ToList();
            return new ManyToManyTestData(articles, allJoins, allTags);
        }

        protected sealed class ManyToManyTestData
        {
            public List<Article> Articles { get; }
            public List<IdentifiableArticleTag> ArticleTags { get; }
            public List<Tag> Tags { get; }

            public ManyToManyTestData(List<Article> articles, List<IdentifiableArticleTag> articleTags, List<Tag> tags)
            {
                Articles = articles;
                ArticleTags = articleTags;
                Tags = tags;
            }

            public void Deconstruct(out List<Article> articles, out List<IdentifiableArticleTag> articleTags, out List<Tag> tags)
            {
                articles = Articles;
                articleTags = ArticleTags;
                tags = Tags;
            }
        }
    }
}
