using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.AttributeBuilderTests
{
    public sealed class When_Calling_Build: HasManyBuilderSpecificationBase
    {
        AttributeBuilder<UnAnnotatedProduct> _attributeBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _property = typeof(UnAnnotatedProduct).GetProperty("Name");

            _attributeBuilder = new AttributeBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property);
        }

        protected override async Task When()
        {
            await base.When();

            _attributeBuilder.Build();
        }

        [Then]
        public void It_Should_Add_Attribute_To_ResourceContext()
        {            
            Assert.Single(_resourceContext.Attributes);
        }

        [Then]
        public void It_Should_Associate_Attribute_With_Property()
        {
            Assert.Equal(_property, _resourceContext.Attributes[0].PropertyInfo);
        }

        [Then]
        public void It_Should_Format_PublicAttributeName_According_To_NamingStrategy()
        {
            Assert.Equal("name", _resourceContext.Attributes[0].PublicAttributeName);
        }

        [Then]
        public void It_Should_Apply_Default_Capabilities()
        {
            Assert.Equal(_options.DefaultAttrCapabilities, _resourceContext.Attributes[0].Capabilities);
            Assert.True(_resourceContext.Attributes[0].HasExplicitCapabilities);
        }
    }
}
