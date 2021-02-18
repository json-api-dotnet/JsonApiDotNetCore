using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor
{
    public sealed class ManyToManyOnReturnTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.OnReturn
        };

        [Fact]
        public void OnReturn()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (var _, var _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, var _, List<Tag> tags) = CreateDummyData();

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
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (var _, var _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, var _, List<Tag> tags) = CreateDummyData();

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
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (var _, var _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, var _, var _) = CreateDummyData();

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
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (var _, var _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, var _, var _) = CreateDummyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        private (List<Article>, List<ArticleTag>, List<Tag>) CreateDummyData()
        {
            List<Tag> tagsSubset = _tagFaker.Generate(3);
            List<ArticleTag> joinsSubSet = _articleTagFaker.Generate(3);
            Article articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet.ToHashSet();

            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            List<Tag> allTags = _tagFaker.Generate(3).Concat(tagsSubset).ToList();
            List<ArticleTag> completeJoin = _articleTagFaker.Generate(6);

            Article articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin.ToHashSet();

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            List<ArticleTag> allJoins = joinsSubSet.Concat(completeJoin).ToList();

            var articles = new List<Article>
            {
                articleTagsSubset,
                articleWithAllTags
            };

            return (articles, allJoins, allTags);
        }
    }
}
