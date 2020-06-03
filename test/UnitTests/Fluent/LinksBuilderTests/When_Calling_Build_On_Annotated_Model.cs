using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Models.Links;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.LinksBuilderTests
{
    public sealed class When_Calling_Build_On_Annotated_Model : LinksBuilderSpecificationBase
    {
        LinksBuilder<AnnotatedProduct> _linksBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(AnnotatedProduct);

            _linksAttribute = typeof(AnnotatedProduct).GetCustomAttribute(typeof(LinksAttribute)) as LinksAttribute;

            _linksBuilder = new LinksBuilder<AnnotatedProduct>(_resourceContext,
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
        public void It_Should_Override_TopLevelLinks()
        {            
            Assert.Equal(_topLevelLinks, _resourceContext.TopLevelLinks);            
        }

        [Then]
        public void It_Should_Override_ResourceLinks()
        {
            Assert.Equal(_resourceLinks, _resourceContext.ResourceLinks);
        }

        [Then]
        public void It_Should_Override_RelationshipLinks()
        {
            Assert.Equal(_relationshipLinks, _resourceContext.RelationshipLinks);
        }
    }
}
