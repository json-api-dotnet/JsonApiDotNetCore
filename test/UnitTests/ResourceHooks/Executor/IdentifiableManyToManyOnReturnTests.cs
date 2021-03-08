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
    public sealed class IdentifiableManyToManyOnReturnTests : HooksTestsSetup
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
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, List<IdentifiableArticleTag> joins, List<Tag> tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());

            joinResourceMock.Verify(
                rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>(collection => !collection.Except(joins).Any()), ResourcePipeline.Get), Times.Once());

            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_GetRelationship()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, List<IdentifiableArticleTag> joins, List<Tag> tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.GetRelationship);

            // Assert
            articleResourceMock.Verify(
                rd => rd.OnReturn(It.Is<HashSet<Article>>(collection => !collection.Except(articles).Any()), ResourcePipeline.GetRelationship), Times.Once());

            joinResourceMock.Verify(
                rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>(collection => !collection.Except(joins).Any()), ResourcePipeline.GetRelationship),
                Times.Once());

            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.GetRelationship),
                Times.Once());

            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, List<IdentifiableArticleTag> joins, List<Tag> tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            joinResourceMock.Verify(
                rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>(collection => !collection.Except(joins).Any()), ResourcePipeline.Get), Times.Once());

            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Children_Hooks_Implemented()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, _, List<Tag> tags) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Grand_Children_Hooks_Implemented()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, List<IdentifiableArticleTag> joins, _) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get), Times.Once());

            joinResourceMock.Verify(
                rd => rd.OnReturn(It.Is<HashSet<IdentifiableArticleTag>>(collection => !collection.Except(joins).Any()), ResourcePipeline.Get), Times.Once());

            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Descendant_Hooks_Implemented()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, _, _) = CreateIdentifiableManyToManyData();

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
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            IHooksDiscovery<IdentifiableArticleTag> joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                    Mock<IResourceHookContainer<IdentifiableArticleTag>> joinResourceMock, Mock<IResourceHookContainer<Tag>> tagResourceMock) =
                CreateTestObjectsC(articleDiscovery, joinDiscovery, tagDiscovery);

            (List<Article> articles, _, _) = CreateIdentifiableManyToManyData();

            // Act
            hookExecutor.OnReturn(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }
    }
}
