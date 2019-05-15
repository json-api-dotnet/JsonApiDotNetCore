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
        [Fact]
        public void AfterRead()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);

            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.AfterRead(articles, ResourceAction.Get, false), Times.Once());
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get, true), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Parent_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            tagResourceMock.Verify(rd => rd.AfterRead(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get, true), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Children_Hooks_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var tagResourceMock) = CreateTestObjects(articleDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateManyToManyData();

            // act
            hookExecutor.AfterRead(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.AfterRead(articles, ResourceAction.Get, false), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, tagResourceMock);
        }

        [Fact]
        public void AfterRead_Without_Any_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, NoHooks);
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

