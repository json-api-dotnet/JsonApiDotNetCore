using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyThroughBuilderTests
{
    public sealed class When_Calling_PublicName_With_Null_Value : HasManyThroughBuilderSpecificationBase
    {
        HasManyThroughBuilder<UnAnnotatedProduct> _hasManyThroughBuilder;
        Exception _exception;

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

            try
            {
                _hasManyThroughBuilder.PublicName(null);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }            
        }

        [Then]
        public void It_Should_Throw_ArgumentNullException()
        {
            Assert.IsType<ArgumentNullException>(_exception);
        }
    }
}
