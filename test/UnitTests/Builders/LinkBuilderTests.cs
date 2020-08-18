using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.RequestServices.Contracts;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;
using JsonApiDotNetCore.Serialization.Server.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace UnitTests
{
    public sealed class LinkBuilderTests
    {
        private readonly IPaginationContext _paginationContext;
        private readonly Mock<IResourceGraph> _provider = new Mock<IResourceGraph>();
        private readonly IRequestQueryStringAccessor _queryStringAccessor = new FakeRequestQueryStringAccessor("?foo=bar");
        private const string _host = "http://www.example.com";
        private const int _primaryId = 123;
        private const string _relationshipName = "author";
        private const string _topSelf = "http://www.example.com/articles?foo=bar";
        private const string _topResourceSelf = "http://www.example.com/articles/123?foo=bar";
        private const string _topRelatedSelf = "http://www.example.com/articles/123/author?foo=bar";
        private const string _resourceSelf = "http://www.example.com/articles/123";
        private const string _relSelf = "http://www.example.com/articles/123/relationships/author";
        private const string _relRelated = "http://www.example.com/articles/123/author";

        public LinkBuilderTests()
        {
            _paginationContext = GetPaginationContext();
        }

        [Theory]
        [InlineData(Links.All, Links.NotConfigured, _resourceSelf)]
        [InlineData(Links.Self, Links.NotConfigured, _resourceSelf)]
        [InlineData(Links.None, Links.NotConfigured, null)]
        [InlineData(Links.All, Links.Self, _resourceSelf)]
        [InlineData(Links.Self, Links.Self, _resourceSelf)]
        [InlineData(Links.None, Links.Self, _resourceSelf)]
        [InlineData(Links.All, Links.None, null)]
        [InlineData(Links.Self, Links.None, null)]
        [InlineData(Links.None, Links.None, null)]
        public void BuildResourceLinks_GlobalAndResourceConfiguration_ExpectedResult(Links global, Links resource, object expectedResult)
        {
            // Arrange
            var config = GetConfiguration(resourceLinks: global);
            var primaryResource = GetArticleResourceContext(resourceLinks: resource);
            _provider.Setup(m => m.GetResourceContext("articles")).Returns(primaryResource);
            var builder = new LinkBuilder(config, GetRequestManager(), null, _provider.Object, _queryStringAccessor);

            // Act
            var links = builder.GetResourceLinks("articles", _primaryId.ToString());

            // Assert
            if (expectedResult == null)
                Assert.Null(links);
            else
                Assert.Equal(_resourceSelf, links.Self);
        }

        [Theory]
        [InlineData(Links.All, Links.NotConfigured, Links.NotConfigured, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.NotConfigured, Links.All, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.NotConfigured, Links.Self, _relSelf, null)]
        [InlineData(Links.All, Links.NotConfigured, Links.Related, null, _relRelated)]
        [InlineData(Links.All, Links.NotConfigured, Links.None, null, null)]
        [InlineData(Links.All, Links.All, Links.NotConfigured, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.All, Links.All, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.All, Links.Self, _relSelf, null)]
        [InlineData(Links.All, Links.All, Links.Related, null, _relRelated)]
        [InlineData(Links.All, Links.All, Links.None, null, null)]
        [InlineData(Links.All, Links.Self, Links.NotConfigured, _relSelf, null)]
        [InlineData(Links.All, Links.Self, Links.All, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.Self, Links.Self, _relSelf, null)]
        [InlineData(Links.All, Links.Self, Links.Related, null, _relRelated)]
        [InlineData(Links.All, Links.Self, Links.None, null, null)]
        [InlineData(Links.All, Links.Related, Links.NotConfigured, null, _relRelated)]
        [InlineData(Links.All, Links.Related, Links.All, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.Related, Links.Self, _relSelf, null)]
        [InlineData(Links.All, Links.Related, Links.Related, null, _relRelated)]
        [InlineData(Links.All, Links.Related, Links.None, null, null)]
        [InlineData(Links.All, Links.None, Links.NotConfigured, null, null)]
        [InlineData(Links.All, Links.None, Links.All, _relSelf, _relRelated)]
        [InlineData(Links.All, Links.None, Links.Self, _relSelf, null)]
        [InlineData(Links.All, Links.None, Links.Related, null, _relRelated)]
        [InlineData(Links.All, Links.None, Links.None, null, null)]
        public void BuildRelationshipLinks_GlobalResourceAndAttrConfiguration_ExpectedLinks(
            Links global, Links resource, Links relationship, object expectedSelfLink, object expectedRelatedLink)
        {
            // Arrange
            var config = GetConfiguration(relationshipLinks: global);
            var primaryResource = GetArticleResourceContext(relationshipLinks: resource);
            _provider.Setup(m => m.GetResourceContext(typeof(Article))).Returns(primaryResource);
            var builder = new LinkBuilder(config, GetRequestManager(), null, _provider.Object, _queryStringAccessor);
            var attr = new HasOneAttribute(links: relationship) { RightType = typeof(Author), PublicName = "author" };

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
        [InlineData(Links.All, Links.NotConfigured, _topSelf, true)]
        [InlineData(Links.All, Links.All, _topSelf, true)]
        [InlineData(Links.All, Links.Self, _topSelf, false)]
        [InlineData(Links.All, Links.Paging, null, true)]
        [InlineData(Links.All, Links.None, null, false)]
        [InlineData(Links.Self, Links.NotConfigured, _topSelf, false)]
        [InlineData(Links.Self, Links.All, _topSelf, true)]
        [InlineData(Links.Self, Links.Self, _topSelf, false)]
        [InlineData(Links.Self, Links.Paging, null, true)]
        [InlineData(Links.Self, Links.None, null, false)]
        [InlineData(Links.Paging, Links.NotConfigured, null, true)]
        [InlineData(Links.Paging, Links.All, _topSelf, true)]
        [InlineData(Links.Paging, Links.Self, _topSelf, false)]
        [InlineData(Links.Paging, Links.Paging, null, true)]
        [InlineData(Links.Paging, Links.None, null, false)]
        [InlineData(Links.None, Links.NotConfigured, null, false)]
        [InlineData(Links.None, Links.All, _topSelf, true)]
        [InlineData(Links.None, Links.Self, _topSelf, false)]
        [InlineData(Links.None, Links.Paging, null, true)]
        [InlineData(Links.None, Links.None, null, false)]
        [InlineData(Links.All, Links.Self, _topResourceSelf, false)]
        [InlineData(Links.Self, Links.Self, _topResourceSelf, false)]
        [InlineData(Links.Paging, Links.Self, _topResourceSelf, false)]
        [InlineData(Links.None, Links.Self, _topResourceSelf, false)]
        [InlineData(Links.All, Links.Self, _topRelatedSelf, false)]
        [InlineData(Links.Self, Links.Self, _topRelatedSelf, false)]
        [InlineData(Links.Paging, Links.Self, _topRelatedSelf, false)]
        [InlineData(Links.None, Links.Self, _topRelatedSelf, false)]
        public void BuildTopLevelLinks_GlobalAndResourceConfiguration_ExpectedLinks(
            Links global, Links resource, string expectedSelfLink, bool pages)
        {
            // Arrange
            var config = GetConfiguration(topLevelLinks: global);
            var primaryResource = GetArticleResourceContext(topLevelLinks: resource);
            _provider.Setup(m => m.GetResourceContext<Article>()).Returns(primaryResource);

            bool usePrimaryId = expectedSelfLink != _topSelf;
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
                if (pages)
                {
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=2", links.Self);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=1", links.First);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=1", links.Prev);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=3", links.Next);
                    Assert.Equal($"{_host}/articles?foo=bar&page[size]=10&page[number]=3", links.Last);
                }
                else
                {
                    Assert.Equal(links.Self , expectedSelfLink);
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
            mock.Setup(m => m.Relationship).Returns(relationshipName != null ? new HasOneAttribute(relationshipName) : null);
            mock.Setup(m => m.PrimaryResource).Returns(resourceContext);
            return mock.Object;
        }

        private IJsonApiOptions GetConfiguration(Links resourceLinks = Links.All, Links topLevelLinks = Links.All, Links relationshipLinks = Links.All)
        {
            var config = new Mock<IJsonApiOptions>();
            config.Setup(m => m.TopLevelLinks).Returns(topLevelLinks);
            config.Setup(m => m.ResourceLinks).Returns(resourceLinks);
            config.Setup(m => m.RelationshipLinks).Returns(relationshipLinks);
            config.Setup(m => m.DefaultPageSize).Returns(new PageSize(25));
            return config.Object;
        }

        private IPaginationContext GetPaginationContext()
        {
            var mock = new Mock<IPaginationContext>();
            mock.Setup(x => x.PageNumber).Returns(new PageNumber(2));
            mock.Setup(x => x.PageSize).Returns(new PageSize(10));
            mock.Setup(x => x.TotalPageCount).Returns(3);

            return mock.Object;
        }

        private ResourceContext GetArticleResourceContext(Links resourceLinks = Links.NotConfigured,
            Links topLevelLinks = Links.NotConfigured,
            Links relationshipLinks = Links.NotConfigured)
        {
            return new ResourceContext
            {
                ResourceLinks = resourceLinks,
                TopLevelLinks = topLevelLinks,
                RelationshipLinks = relationshipLinks,
                ResourceName = "articles"
            };
        }

        private sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
        {
            public QueryString QueryString { get; }
            public IQueryCollection Query { get; }

            public FakeRequestQueryStringAccessor(string queryString)
            {
                QueryString = new QueryString(queryString);
                Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
            }
        }
    }
}
