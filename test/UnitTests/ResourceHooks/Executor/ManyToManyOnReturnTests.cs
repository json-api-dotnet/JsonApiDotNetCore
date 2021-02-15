using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor
{
    public sealed class ManyToManyOnReturnTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks = { ResourceHook.OnReturn };

        private (List<Article>, List<ArticleTag>, List<Tag>) CreateDummyData()
        {
            var tagsSubset = _tagFaker.Generate(3);
            var joinsSubSet = _articleTagFaker.Generate(3);
            var articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet.ToHashSet();
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            var allTags = _tagFaker.Generate(3).Concat(tagsSubset).ToList();
            var completeJoin = _articleTagFaker.Generate(6);

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin.ToHashSet();

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            var allJoins = joinsSubSet.Concat(completeJoin).ToList();

            var articles = new List<Article> { articleTagsSubset, articleWithAllTags };
            return (articles, allJoins, allTags);
        }

        [Fact]
        public void OnReturn()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, _, tags) = CreateDummyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, _, tags) = CreateDummyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Children_Hooks_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, _, _) = CreateDummyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Hook_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            var (articles, _, _) = CreateDummyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }
    }
}

