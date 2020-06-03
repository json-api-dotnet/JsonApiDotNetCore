using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyThroughBuilderTests
{
    public sealed class When_Calling_CanInclude : HasManyThroughBuilderSpecificationBase
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

            _hasManyThroughBuilder.CanInclude(false);
        }

        [Then]
        public void It_Should_Configure_CanInclude()
        {            
            Assert.False(((HasManyThroughAttribute)_resourceContext.Relationships[0]).CanInclude);            
        }        
    }
}
