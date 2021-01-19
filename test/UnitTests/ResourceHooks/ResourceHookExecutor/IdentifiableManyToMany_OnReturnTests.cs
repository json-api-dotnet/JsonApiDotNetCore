using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class IdentifiableManyToMany_OnReturnTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.OnReturn };

        [Fact]
        public void OnReturn()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            var (articles, joins, tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourcePipeline.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>((collection) => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_GetRelationship()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            var (articles, joins, tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.GetRelationship);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Article>>((collection) => !collection.Except(articles).Any()), ResourcePipeline.GetRelationship), Times.Once());
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourcePipeline.GetRelationship), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>((collection) => !collection.Except(tags).Any()), ResourcePipeline.GetRelationship), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            var (articles, joins, tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourcePipeline.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>((collection) => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Children_Hooks_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);

            var (articles, _, tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>((collection) => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Grand_Children_Hooks_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            var (articles, joins, _) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Descendant_Hooks_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            var (articles, _, _) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Hook_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, hookExecutor, articleResourceMock, joinResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            var (articles, _, _) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }
    }
}
