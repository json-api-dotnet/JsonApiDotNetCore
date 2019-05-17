using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class ManyToMany_AfterReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.AfterRead };

        [Fact]
        public void AfterRead()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.AfterRead(It.IsAny<HashSet<Article>>(), ResourceAction.Get, false), Times.Once());
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<HashSet<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get, true), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Parent_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<HashSet<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get, true), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Children_Hooks_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.AfterRead(It.IsAny<HashSet<Article>>(), ResourceAction.Get, false), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Any_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }
    }
}

