using JsonApiDotNetCore.Fluent;
using System;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.AttributeBuilderTests
{
    public sealed class When_Calling_PublicName_With_Null_Value : HasManyBuilderSpecificationBase
    {
        AttributeBuilder<UnAnnotatedProduct> _attributeBuilder;
        Exception _exception;

        protected override async Task Given()
        {
            await base.Given();

            _property = typeof(UnAnnotatedProduct).GetProperty("Name");

            _attributeBuilder = new AttributeBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property);

            _attributeBuilder.Build();
        }

        protected override async Task When()
        {
            await base.When();

            try
            {
                _attributeBuilder.PublicName(null);
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
