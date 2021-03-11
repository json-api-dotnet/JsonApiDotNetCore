using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore;
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

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, List<Tag> tags) = CreateDummyData();

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

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, List<Tag> tags) = CreateDummyData();

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

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, _) = CreateDummyData();

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

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, _) = CreateDummyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        private (List<Article>, List<Tag>) CreateDummyData()
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
    }
}
