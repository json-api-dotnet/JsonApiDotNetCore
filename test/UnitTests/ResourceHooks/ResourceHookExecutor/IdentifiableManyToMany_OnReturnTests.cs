using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class IdentifiableManyToMany_OnReturnTests : HooksTestsSetup
    {
        [Fact]
        public void OnReturn()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.OnReturn(articles, ResourceAction.Get), Times.Once());
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourceAction.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Parent_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, NoHooks);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourceAction.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Children_Hooks_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(AllHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);

            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.OnReturn(articles, ResourceAction.Get), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Grand_Children_Hooks_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(AllHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.OnReturn(articles, ResourceAction.Get), Times.Once());
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Descendant_Hooks_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(AllHooks, NoHooks);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // assert
            articleResourceMock.Verify(rd => rd.OnReturn(articles, ResourceAction.Get), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Any_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, NoHooks);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, NoHooks);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, NoHooks);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.Get);

            // asert
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }
    }
}

