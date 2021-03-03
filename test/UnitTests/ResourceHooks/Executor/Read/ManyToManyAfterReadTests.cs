using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Read
{
    public sealed class ManyToManyAfterReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks = { ResourceHook.AfterRead };

        [Fact]
        public void AfterRead()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, tags) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.AfterRead(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get, false), Times.Once());
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get, true), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, tags) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get, true), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Children_Hooks_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, _) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.AfterRead(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get, false), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Any_Hook_Implemented()
        {
            // Arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, articleResourceMock, tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            var (articles, _) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }
    }
}

