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
        protected readonly Faker<TodoItem> TodoFaker;
        protected readonly Faker<Person> PersonFaker;
        protected readonly Faker<Article> ArticleFaker;
        protected readonly Faker<Tag> TagFaker;
        protected readonly Faker<ArticleTag> ArticleTagFaker;
        protected readonly Faker<IdentifiableArticleTag> IdentifiableArticleTagFaker;
        protected readonly Faker<Passport> PassportFaker;
        protected IResourceGraph ResourceGraph { get; }
        protected ResourceHook[] NoHooks { get; } = new ResourceHook[0];

        protected ResourceHook[] EnableDbValues { get; } =
        {
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeUpdateRelationship
        };

        protected ResourceHook[] DisableDbValues { get; } = new ResourceHook[0];

        public HooksDummyData()
        {
            ResourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TodoItem>().Add<Person>().Add<Passport>()
                .Add<Article>().Add<IdentifiableArticleTag>().Add<Tag>().Build();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            TodoFaker = new Faker<TodoItem>()
                .RuleFor(x => x.Id, f => f.UniqueIndex + 1);

            PersonFaker = new Faker<Person>()
                .RuleFor(x => x.Id, f => f.UniqueIndex + 1);

            ArticleFaker = new Faker<Article>()
                .RuleFor(x => x.Id, f => f.UniqueIndex + 1);

            TagFaker = new Faker<Tag>()
                .RuleFor(x => x.Id, f => f.UniqueIndex + 1);

            ArticleTagFaker = new Faker<ArticleTag>();

            IdentifiableArticleTagFaker = new Faker<IdentifiableArticleTag>()
                .RuleFor(x => x.Id, f => f.UniqueIndex + 1);

            PassportFaker = new Faker<Passport>()
                .RuleFor(x => x.Id, f => f.UniqueIndex + 1);

            // @formatter:wrap_chained_method_calls restore
            // @formatter:keep_existing_linebreaks restore
        }

        protected List<TodoItem> CreateTodoWithToOnePerson()
        {
            TodoItem todoItem = TodoFaker.Generate();
            Person person = PersonFaker.Generate();
            List<TodoItem> todoList = todoItem.AsList();
            person.OneToOneTodoItem = todoItem;
            todoItem.OneToOnePerson = person;
            return todoList;
        }

        protected HashSet<TodoItem> CreateTodoWithOwner()
        {
            TodoItem todoItem = TodoFaker.Generate();
            Person person = PersonFaker.Generate();

            var todoList = new HashSet<TodoItem>
            {
                todoItem
            };

            person.AssignedTodoItems = todoList;
            todoItem.Owner = person;
            return todoList;
        }

        protected (List<Article>, List<ArticleTag>, List<Tag>) CreateManyToManyData()
        {
            List<Tag> tagsSubset = TagFaker.Generate(3);
            List<ArticleTag> joinsSubSet = ArticleTagFaker.Generate(3);
            Article articleTagsSubset = ArticleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet.ToHashSet();

            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            List<Tag> allTags = TagFaker.Generate(3).Concat(tagsSubset).ToList();
            List<ArticleTag> completeJoin = ArticleTagFaker.Generate(6);

            Article articleWithAllTags = ArticleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin.ToHashSet();

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            List<ArticleTag> allJoins = joinsSubSet.Concat(completeJoin).ToList();

            List<Article> articles = ArrayFactory.Create(articleTagsSubset, articleWithAllTags).ToList();
            return (articles, allJoins, allTags);
        }

        protected (List<Article>, List<IdentifiableArticleTag>, List<Tag>) CreateIdentifiableManyToManyData()
        {
            List<Tag> tagsSubset = TagFaker.Generate(3);
            List<IdentifiableArticleTag> joinsSubSet = IdentifiableArticleTagFaker.Generate(3);
            Article articleTagsSubset = ArticleFaker.Generate();
            articleTagsSubset.IdentifiableArticleTags = joinsSubSet.ToHashSet();

            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            List<Tag> allTags = TagFaker.Generate(3).Concat(tagsSubset).ToList();
            List<IdentifiableArticleTag> completeJoin = IdentifiableArticleTagFaker.Generate(6);

            Article articleWithAllTags = ArticleFaker.Generate();
            articleWithAllTags.IdentifiableArticleTags = joinsSubSet.ToHashSet();

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            List<IdentifiableArticleTag> allJoins = joinsSubSet.Concat(completeJoin).ToList();
            List<Article> articles = ArrayFactory.Create(articleTagsSubset, articleWithAllTags).ToList();
            return (articles, allJoins, allTags);
        }
    }
}
