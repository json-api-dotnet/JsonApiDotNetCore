using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Read
{
    public sealed class ManyToManyAfterReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.AfterRead
        };

        [Fact]
        public void AfterRead()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, List<Tag> tags) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            articleResourceMock.Verify(rd => rd.AfterRead(It.IsAny<HashSet<Article>>(), ResourcePipeline.Get, false), Times.Once());

            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get, true),
                Times.Once());

            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, List<Tag> tags) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<HashSet<Tag>>(collection => !collection.Except(tags).Any()), ResourcePipeline.Get, true),
                Times.Once());

            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Children_Hooks_Implemented()
        {
            // Arrange
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, _) = CreateManyToManyData();

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
            IHooksDiscovery<Article> articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            IHooksDiscovery<Tag> tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Article>> articleResourceMock,
                Mock<IResourceHookContainer<Tag>> tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (List<Article> articles, _) = CreateManyToManyData();

            // Act
            hookExecutor.AfterRead(articles, ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }
    }
}
