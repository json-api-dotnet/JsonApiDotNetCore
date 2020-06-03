using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.LinksBuilderTests
{
    public sealed class When_Calling_Build: LinksBuilderSpecificationBase
    {
        LinksBuilder<UnAnnotatedProduct> _linksBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(UnAnnotatedProduct);

            _linksBuilder = new LinksBuilder<UnAnnotatedProduct>(_resourceContext,
                                                                 _options,
                                                                 _topLevelLinks,
                                                                 _resourceLinks,
                                                                 _relationshipLinks);
        }                                                        

        protected override async Task When()
        {
            await base.When();

            _linksBuilder.Build();
        }

        [Then]
        public void It_Should_Override_TopLevelLinks_On_ResourceContext()
        {            
            Assert.Equal(_topLevelLinks, _resourceContext.TopLevelLinks);            
        }

        [Then]
        public void It_Should_Override_ResourceLinks_On_ResourceContext()
        {
            Assert.Equal(_resourceLinks, _resourceContext.ResourceLinks);
        }

        [Then]
        public void It_Should_Override_RelationshipLinks_On_ResourceContext()
        {
            Assert.Equal(_relationshipLinks, _resourceContext.RelationshipLinks);
        }
    }
}
