using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Xunit;

namespace UnitTests
{
    public sealed class LinkBuilderTests
    {
        private readonly IPaginationContext _paginationContext = GetPaginationContext();
        private readonly Mock<IResourceGraph> _provider = new Mock<IResourceGraph>();
        private readonly IRequestQueryStringAccessor _queryStringAccessor = new FakeRequestQueryStringAccessor("?foo=bar&page[size]=10&page[number]=2");
        private const string _host = "http://www.example.com";
        private const int _primaryId = 123;
        private const string _relationshipName = "author";
        private const string _topSelf = "http://www.example.com/articles?foo=bar&page[size]=10&page[number]=2";
        private const string _topResourceSelf = "http://www.example.com/articles/123?foo=bar&page[size]=10&page[number]=2";
        private const string _topRelatedSelf = "http://www.example.com/articles/123/author?foo=bar&page[size]=10&page[number]=2";
        private const string _resourceSelf = "http://www.example.com/articles/123";
        private const string _relSelf = "http://www.example.com/articles/123/relationships/author";
        private const string _relRelated = "http://www.example.com/articles/123/author";

        [Theory]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, _resourceSelf)]
        [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, _resourceSelf)]
        [InlineData(LinkTypes.None, LinkTypes.NotConfigured, null)]
        [InlineData(LinkTypes.All, LinkTypes.Self, _resourceSelf)]
        [InlineData(LinkTypes.Self, LinkTypes.Self, _resourceSelf)]
        [InlineData(LinkTypes.None, LinkTypes.Self, _resourceSelf)]
        [InlineData(LinkTypes.All, LinkTypes.None, null)]
        [InlineData(LinkTypes.Self, LinkTypes.None, null)]
        [InlineData(LinkTypes.None, LinkTypes.None, null)]
        public void BuildResourceLinks_GlobalAndResourceConfiguration_ExpectedResult(LinkTypes global, LinkTypes resource, object expectedResult)
        {
            // Arrange
            var config = GetConfiguration(resourceLinks: global);
            var primaryResource = GetArticleResourceContext(resourceLinks: resource);
            _provider.Setup(m => m.GetResourceContext("articles")).Returns(primaryResource);
            var builder = new LinkBuilder(config, GetRequestManager(), new PaginationContext(), _provider.Object, _queryStringAccessor);

            // Act
            var links = builder.GetResourceLinks("articles", _primaryId.ToString());

            // Assert
            if (expectedResult == null)
                Assert.Null(links);
            else
                Assert.Equal(_resourceSelf, links.Self);
        }

        [Theory]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.NotConfigured, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.All, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Self, _relSelf, null)]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Related, null, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.None, null, null)]
        [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.NotConfigured, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.All, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.Self, _relSelf, null)]
        [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.Related, null, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.None, null, null)]
        [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.NotConfigured, _relSelf, null)]
        [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.All, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.Self, _relSelf, null)]
        [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.Related, null, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.None, null, null)]
        [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.NotConfigured, null, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.All, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.Self, _relSelf, null)]
        [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.Related, null, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.None, null, null)]
        [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.NotConfigured, null, null)]
        [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.All, _relSelf, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.Self, _relSelf, null)]
        [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.Related, null, _relRelated)]
        [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.None, null, null)]
        public void BuildRelationshipLinks_GlobalResourceAndAttrConfiguration_ExpectedLinks(
            LinkTypes global, LinkTypes resource, LinkTypes relationship, object expectedSelfLink, object expectedRelatedLink)
        {
            // Arrange
            var config = GetConfiguration(relationshipLinks: global);
            var primaryResource = GetArticleResourceContext(relationshipLinks: resource);
            _provider.Setup(m => m.GetResourceContext(typeof(Article))).Returns(primaryResource);
            var builder = new LinkBuilder(config, GetRequestManager(), new PaginationContext(), _provider.Object, _queryStringAccessor);
            var attr = new HasOneAttribute { Links = relationship, RightType = typeof(Author), PublicName = "author" };

            // Act
            var links = builder.GetRelationshipLinks(attr, new Article { Id = _primaryId });

            // Assert
            if (expectedSelfLink == null && expectedRelatedLink == null)
            {
                Assert.Null(links);
            }
            else
            {
                Assert.Equal(expectedSelfLink, links.Self);
                Assert.Equal(expectedRelatedLink, links.Related);
            }
        }

        [Theory]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, _topSelf, true)]
        [InlineData(LinkTypes.All, LinkTypes.All, _topSelf, true)]
        [InlineData(LinkTypes.All, LinkTypes.Self, _topSelf, false)]
        [InlineData(LinkTypes.All, LinkTypes.Paging, null, true)]
        [InlineData(LinkTypes.All, LinkTypes.None, null, false)]
        [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, _topSelf, false)]
        [InlineData(LinkTypes.Self, LinkTypes.All, _topSelf, true)]
        [InlineData(LinkTypes.Self, LinkTypes.Self, _topSelf, false)]
        [InlineData(LinkTypes.Self, LinkTypes.Paging, null, true)]
        [InlineData(LinkTypes.Self, LinkTypes.None, null, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.NotConfigured, null, true)]
        [InlineData(LinkTypes.Paging, LinkTypes.All, _topSelf, true)]
        [InlineData(LinkTypes.Paging, LinkTypes.Self, _topSelf, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.Paging, null, true)]
        [InlineData(LinkTypes.Paging, LinkTypes.None, null, false)]
        [InlineData(LinkTypes.None, LinkTypes.NotConfigured, null, false)]
        [InlineData(LinkTypes.None, LinkTypes.All, _topSelf, true)]
        [InlineData(LinkTypes.None, LinkTypes.Self, _topSelf, false)]
        [InlineData(LinkTypes.None, LinkTypes.Paging, null, true)]
        [InlineData(LinkTypes.None, LinkTypes.None, null, false)]
        [InlineData(LinkTypes.All, LinkTypes.Self, _topResourceSelf, false)]
        [InlineData(LinkTypes.Self, LinkTypes.Self, _topResourceSelf, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.Self, _topResourceSelf, false)]
        [InlineData(LinkTypes.None, LinkTypes.Self, _topResourceSelf, false)]
        [InlineData(LinkTypes.All, LinkTypes.Self, _topRelatedSelf, false)]
        [InlineData(LinkTypes.Self, LinkTypes.Self, _topRelatedSelf, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.Self, _topRelatedSelf, false)]
        [InlineData(LinkTypes.None, LinkTypes.Self, _topRelatedSelf, false)]
        public void BuildTopLevelLinks_GlobalAndResourceConfiguration_ExpectedLinks(
            LinkTypes global, LinkTypes resource, string expectedSelfLink, bool pages)
        {
            // Arrange
            var config = GetConfiguration(topLevelLinks: global);
            var primaryResource = GetArticleResourceContext(topLevelLinks: resource);
            _provider.Setup(m => m.GetResourceContext<Article>()).Returns(primaryResource);

            bool usePrimaryId = expectedSelfLink != null && expectedSelfLink.Contains("123");
            string relationshipName = expectedSelfLink == _topRelatedSelf ? _relationshipName : null;
            IJsonApiRequest request = GetRequestManager(primaryResource, usePrimaryId, relationshipName);

            var builder = new LinkBuilder(config, request, _paginationContext, _provider.Object, _queryStringAccessor);

            // Act
            var links = builder.GetTopLevelLinks();

            // Assert
            if (!pages && expectedSelfLink == null)
            {
                Assert.Null(links);
            }
            else
            {
                Assert.Equal(links.Self, expectedSelfLink);

                if (pages)
                {
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10", links.First);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10", links.Prev);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=3", links.Next);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=3", links.Last);
                }
                else
                {
                    Assert.Null(links.First);
                    Assert.Null(links.Prev);
                    Assert.Null(links.Next);
                    Assert.Null(links.Last);
                }
            }
        }

        private IJsonApiRequest GetRequestManager(ResourceContext resourceContext = null, bool usePrimaryId = false, string relationshipName = null)
        {
            var mock = new Mock<IJsonApiRequest>();
            mock.Setup(m => m.BasePath).Returns(_host);
            mock.Setup(m => m.PrimaryId).Returns(usePrimaryId ? _primaryId.ToString() : null);
            mock.Setup(m => m.Relationship).Returns(relationshipName != null ? new HasOneAttribute {PublicName = relationshipName} : null);
            mock.Setup(m => m.PrimaryResource).Returns(resourceContext);
            mock.Setup(m => m.IsCollection).Returns(true);
            return mock.Object;
        }

        private IJsonApiOptions GetConfiguration(LinkTypes resourceLinks = LinkTypes.All, LinkTypes topLevelLinks = LinkTypes.All, LinkTypes relationshipLinks = LinkTypes.All)
        {
            var config = new Mock<IJsonApiOptions>();
            config.Setup(m => m.TopLevelLinks).Returns(topLevelLinks);
            config.Setup(m => m.ResourceLinks).Returns(resourceLinks);
            config.Setup(m => m.RelationshipLinks).Returns(relationshipLinks);
            config.Setup(m => m.DefaultPageSize).Returns(new PageSize(25));
            return config.Object;
        }

        private static IPaginationContext GetPaginationContext()
        {
            var mock = new Mock<IPaginationContext>();
            mock.Setup(x => x.PageNumber).Returns(new PageNumber(2));
            mock.Setup(x => x.PageSize).Returns(new PageSize(10));
            mock.Setup(x => x.TotalPageCount).Returns(3);

            return mock.Object;
        }

        private ResourceContext GetArticleResourceContext(LinkTypes resourceLinks = LinkTypes.NotConfigured,
            LinkTypes topLevelLinks = LinkTypes.NotConfigured,
            LinkTypes relationshipLinks = LinkTypes.NotConfigured)
        {
            return new ResourceContext
            {
                ResourceLinks = resourceLinks,
                TopLevelLinks = topLevelLinks,
                RelationshipLinks = relationshipLinks,
                PublicName = "articles"
            };
        }

        private sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
        {
            public IQueryCollection Query { get; }

            public FakeRequestQueryStringAccessor(string queryString)
            {
                Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
            }
        }
    }
}
