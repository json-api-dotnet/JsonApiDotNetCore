using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyThroughBuilderTests
{
    public sealed class When_Calling_PublicName : HasManyThroughBuilderSpecificationBase
    {
        HasManyThroughBuilder<UnAnnotatedProduct> _hasManyThroughBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(UnAnnotatedProduct);

            _property = typeof(UnAnnotatedProduct).GetProperty("Categories");

            _throughProperty = typeof(UnAnnotatedProduct).GetProperty("ProductCategories");        

            _hasManyThroughBuilder = new HasManyThroughBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property, _throughProperty);

            _hasManyThroughBuilder.Build();
        }

        protected override async Task When()
        {
            await base.When();

            _hasManyThroughBuilder.PublicName("product-categories");
        }

        [Then]
        public void It_Should_Override_PublicRelationshipName_Default_Format()
        {
            Assert.Equal("product-categories", _resourceContext.Relationships[0].PublicRelationshipName);
        }
    }
}
