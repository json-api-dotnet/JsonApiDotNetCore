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
        private readonly ResourceHook[] targetHooks = { ResourceHook.OnReturn };

        [Fact]
        public void OnReturn()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
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
        public void OnReturn_GetRelationship()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var articleResourceMock,
                var joinResourceMock, var tagResourceMock) = CreateTestObjects(articleDiscovery, joinDiscovery, tagDiscovery);
            (var articles, var joins, var tags) = CreateIdentifiableManyToManyData();

            // act
            hookExecutor.OnReturn(articles, ResourceAction.GetRelationship);

            // assert
            joinResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<IdentifiableArticleTag>>((collection) => !collection.Except(joins).Any()), ResourceAction.GetRelationship), Times.Once());
            tagResourceMock.Verify(rd => rd.OnReturn(It.Is<IEnumerable<Tag>>((collection) => !collection.Except(tags).Any()), ResourceAction.GetRelationship), Times.Once());
            VerifyNoOtherCalls(articleResourceMock, joinResourceMock, tagResourceMock);
        }

        [Fact]
        public void OnReturn_Without_Parent_Hook_Implemented()
        {
            // arrange
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
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
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(targetHooks, DisableDbValues);
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
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(targetHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
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
            var articleDiscovery = SetDiscoverableHooks<Article>(targetHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
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
            var articleDiscovery = SetDiscoverableHooks<Article>(NoHooks, DisableDbValues);
            var joinDiscovery = SetDiscoverableHooks<IdentifiableArticleTag>(NoHooks, DisableDbValues);
            var tagDiscovery = SetDiscoverableHooks<Tag>(NoHooks, DisableDbValues);
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

