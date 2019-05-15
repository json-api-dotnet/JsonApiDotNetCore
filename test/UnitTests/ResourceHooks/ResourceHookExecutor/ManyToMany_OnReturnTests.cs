using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class ManyToMany_OnReturnTests : HooksTestsSetup
    {
        (List<Article>, List<ArticleTag>, List<Tag>) CreateDummyData()
        {
            var tagsSubset = _tagFaker.Generate(3).ToList();
            var joinsSubSet = _articleTagFaker.Generate(3).ToList();
            var articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet;
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            var allTags = _tagFaker.Generate(3).ToList().Concat(tagsSubset).ToList();
            var completeJoin = _articleTagFaker.Generate(6).ToList();

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin;

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            var allJoins = joinsSubSet.Concat(completeJoin).ToList();

            var articles = new List<Article>() { articleTagsSubset, articleWithAllTags };
            return (articles, allJoins, allTags);
        }

        [Fact]
        public void OnReturn()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateDummyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.OnReturn(articles, ResourceAction.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Parent_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateDummyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // asser
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Children_Hooks_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateDummyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.OnReturn(articles, ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (var articles, var joins, var tags) = CreateDummyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }
    }
}

