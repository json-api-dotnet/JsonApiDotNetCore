using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;
using JsonApiDotNetCore.QueryServices.Contracts;
using JsonApiDotNetCore.Serialization.Response;

namespace UnitTests
{
    public class LinkBuilderTests
    {
        private readonly IPageQueryService _pageManager;
        private readonly Mock<IContextEntityProvider> _provider = new Mock<IContextEntityProvider>();
        private const string _host = "http://www.example.com";
        private const string _topSelf = "http://www.example.com/articles";
        private const string _resourceSelf = "http://www.example.com/articles/123";
        private const string _relSelf = "http://www.example.com/articles/123/relationships/author";
        private const string _relRelated = "http://www.example.com/articles/123/author";

        public LinkBuilderTests()
        {
            _pageManager = GetPageManager();
        }

        [Theory]
        [InlineData(Link.All, Link.NotConfigured, _resourceSelf)]
        [InlineData(Link.Self, Link.NotConfigured, _resourceSelf)]
        [InlineData(Link.None, Link.NotConfigured, null)]
        [InlineData(Link.All, Link.Self, _resourceSelf)]
        [InlineData(Link.Self, Link.Self, _resourceSelf)]
        [InlineData(Link.None, Link.Self, _resourceSelf)]
        [InlineData(Link.All, Link.None, null)]
        [InlineData(Link.Self, Link.None, null)]
        [InlineData(Link.None, Link.None, null)]
        public void BuildResourceLinks_GlobalAndResourceConfiguration_ExpectedResult(Link global, Link resource, object expectedResult)
        {
            // arrange
            var config = GetConfiguration(resourceLinks: global);
            _provider.Setup(m => m.GetContextEntity("articles")).Returns(GetContextEntity<Article>(resourceLinks: resource));
            var builder = new LinkBuilder(config, GetRequestManager(), _pageManager, _provider.Object);

            // act
            var links = builder.GetResourceLinks("articles", "123");

            // assert
            if (expectedResult == null)
                Assert.Null(links);
            else
                Assert.Equal(_resourceSelf, links.Self);
        }



        [Theory]
        [InlineData(Link.All, Link.NotConfigured, Link.NotConfigured, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.NotConfigured, Link.All, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.NotConfigured, Link.Self, _relSelf, null)]
        [InlineData(Link.All, Link.NotConfigured, Link.Related, null, _relRelated)]
        [InlineData(Link.All, Link.NotConfigured, Link.None, null, null)]
        [InlineData(Link.All, Link.All, Link.NotConfigured, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.All, Link.All, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.All, Link.Self, _relSelf, null)]
        [InlineData(Link.All, Link.All, Link.Related, null, _relRelated)]
        [InlineData(Link.All, Link.All, Link.None, null, null)]
        [InlineData(Link.All, Link.Self, Link.NotConfigured, _relSelf, null)]
        [InlineData(Link.All, Link.Self, Link.All, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.Self, Link.Self, _relSelf, null)]
        [InlineData(Link.All, Link.Self, Link.Related, null, _relRelated)]
        [InlineData(Link.All, Link.Self, Link.None, null, null)]
        [InlineData(Link.All, Link.Related, Link.NotConfigured, null, _relRelated)]
        [InlineData(Link.All, Link.Related, Link.All, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.Related, Link.Self, _relSelf, null)]
        [InlineData(Link.All, Link.Related, Link.Related, null, _relRelated)]
        [InlineData(Link.All, Link.Related, Link.None, null, null)]
        [InlineData(Link.All, Link.None, Link.NotConfigured, null, null)]
        [InlineData(Link.All, Link.None, Link.All, _relSelf, _relRelated)]
        [InlineData(Link.All, Link.None, Link.Self, _relSelf, null)]
        [InlineData(Link.All, Link.None, Link.Related, null, _relRelated)]
        [InlineData(Link.All, Link.None, Link.None, null, null)]
        public void BuildRelationshipLinks_GlobalResourceAndAttrConfiguration_ExpectedLinks(Link global,
                                                                                                Link resource,
                                                                                                Link relationship,
                                                                                                object expectedSelfLink,
                                                                                                object expectedRelatedLink)
        {
            // arrange
            var config = GetConfiguration(relationshipLinks: global);
            _provider.Setup(m => m.GetContextEntity(typeof(Article))).Returns(GetContextEntity<Article>(relationshipLinks: resource));
            var builder = new LinkBuilder(config, GetRequestManager(), _pageManager, _provider.Object);
            var attr = new HasOneAttribute(links: relationship) { DependentType = typeof(Author), PublicRelationshipName = "author" };

            // act
            var links = builder.GetRelationshipLinks(attr, new Article { Id = 123 });

            // assert
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
        [InlineData(Link.All, Link.NotConfigured, _topSelf, true)]
        [InlineData(Link.All, Link.All, _topSelf, true)]
        [InlineData(Link.All, Link.Self, _topSelf, false)]
        [InlineData(Link.All, Link.Paging, null, true)]
        [InlineData(Link.All, Link.None, null, null)]
        [InlineData(Link.Self, Link.NotConfigured, _topSelf, false)]
        [InlineData(Link.Self, Link.All, _topSelf, true)]
        [InlineData(Link.Self, Link.Self, _topSelf, false)]
        [InlineData(Link.Self, Link.Paging, null, true)]
        [InlineData(Link.Self, Link.None, null, null)]
        [InlineData(Link.Paging, Link.NotConfigured, null, true)]
        [InlineData(Link.Paging, Link.All, _topSelf, true)]
        [InlineData(Link.Paging, Link.Self, _topSelf, false)]
        [InlineData(Link.Paging, Link.Paging, null, true)]
        [InlineData(Link.Paging, Link.None, null, null)]
        [InlineData(Link.None, Link.NotConfigured, null, false)]
        [InlineData(Link.None, Link.All, _topSelf, true)]
        [InlineData(Link.None, Link.Self, _topSelf, false)]
        [InlineData(Link.None, Link.Paging, null, true)]
        [InlineData(Link.None, Link.None, null, null)]
        public void BuildTopLevelLinks_GlobalAndResourceConfiguration_ExpectedLinks(Link global,
                                                                                    Link resource,
                                                                                    object expectedSelfLink,
                                                                                    bool pages)
        {
            // arrange
            var config = GetConfiguration(topLevelLinks: global);
            var resourceContext = GetContextEntity<Article>(topLevelLinks: resource);
            var builder = new LinkBuilder(config, GetRequestManager(resourceContext), _pageManager, null);

            // act
            var links = builder.GetTopLevelLinks();

            // assert
            if (!pages && expectedSelfLink == null)
            {
                Assert.Null(links);
            }
            else
            {
                Assert.Equal(expectedSelfLink, links.Self);
                Assert.True(CheckPages(links, pages));
            }
        }

        private bool CheckPages(TopLevelLinks links, bool pages)
        {
            if (pages)
            {
                return links.First == $"{_host}/articles?page[size]=10&page[number]=1"
                    && links.Prev == $"{_host}/articles?page[size]=10&page[number]=1"
                    && links.Next == $"{_host}/articles?page[size]=10&page[number]=3"
                    && links.Last == $"{_host}/articles?page[size]=10&page[number]=3";
            }
            return links.First == null && links.Prev == null && links.Next == null && links.Last == null;
        }

        private ICurrentRequest GetRequestManager(ContextEntity resourceContext = null)
        {
            var mock = new Mock<ICurrentRequest>();
            mock.Setup(m => m.BasePath).Returns(_host);
            mock.Setup(m => m.GetRequestResource()).Returns(resourceContext);
            return mock.Object;
        }

        private ILinksConfiguration GetConfiguration(Link resourceLinks = Link.All,
                                                           Link topLevelLinks = Link.All,
                                                           Link relationshipLinks = Link.All)
        {
            var config = new Mock<ILinksConfiguration>();
            config.Setup(m => m.TopLevelLinks).Returns(topLevelLinks);
            config.Setup(m => m.ResourceLinks).Returns(resourceLinks);
            config.Setup(m => m.RelationshipLinks).Returns(relationshipLinks);
            return config.Object;
        }

        private IPageQueryService GetPageManager()
        {
            var mock = new Mock<IPageQueryService>();
            mock.Setup(m => m.ShouldPaginate()).Returns(true);
            mock.Setup(m => m.CurrentPage).Returns(2);
            mock.Setup(m => m.TotalPages).Returns(3);
            mock.Setup(m => m.PageSize).Returns(10);
            return mock.Object;

        }



        private ContextEntity GetContextEntity<TResource>(Link resourceLinks = Link.NotConfigured,
                                                          Link topLevelLinks = Link.NotConfigured,
                                                          Link relationshipLinks = Link.NotConfigured) where TResource : class, IIdentifiable
        {
            return new ContextEntity
            {
                ResourceLinks = resourceLinks,
                TopLevelLinks = topLevelLinks,
                RelationshipLinks = relationshipLinks,
                EntityName = typeof(TResource).Name.Dasherize() + "s"
            };
        }
    }
}
