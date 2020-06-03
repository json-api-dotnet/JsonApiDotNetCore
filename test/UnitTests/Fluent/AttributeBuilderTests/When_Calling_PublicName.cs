using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.AttributeBuilderTests
{
    public sealed class When_Calling_PublicName: HasManyBuilderSpecificationBase
    {
        AttributeBuilder<UnAnnotatedProduct> _attributeBuilder;

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

            _attributeBuilder.PublicName("item-name");            
        }
        
        [Then]
        public void It_Should_Override_PublicAttributeName_Default_Format()
        {
            Assert.Equal("item-name", _resourceContext.Attributes[0].PublicAttributeName);
        }        
    }
}
