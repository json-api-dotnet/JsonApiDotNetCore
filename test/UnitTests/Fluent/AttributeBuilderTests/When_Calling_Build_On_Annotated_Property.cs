using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.AttributeBuilderTests
{
    public sealed class When_Calling_Build_On_Annotated_Property: HasManyBuilderSpecificationBase
    {
        AttributeBuilder<AnnotatedProduct> _attributeBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _property = typeof(AnnotatedProduct).GetProperty("Name");

            _attrAttribute = _property.GetCustomAttribute(typeof(AttrAttribute)) as AttrAttribute;
            _attrAttribute.PropertyInfo = _property;
            _resourceContext.Attributes.Add(_attrAttribute);

            _attributeBuilder = new AttributeBuilder<AnnotatedProduct>(_resourceContext, _options, _property);            
        }

        protected override async Task When()
        {
            await base.When();

            _attributeBuilder.Build();
        }

        [Then]
        public void It_Should_Override_Attribute_On_ResourceContext()
        {            
            Assert.Single(_resourceContext.Attributes);
        }

        [Then]
        public void It_Should_Override_Attribute_Capabilities()
        {
            Assert.NotEqual(_attrAttribute.Capabilities, _resourceContext.Attributes[0].Capabilities);
        }

        [Then]
        public void It_Should_Override_PublicAttributeName()
        {
            Assert.NotEqual(_attrAttribute.PublicAttributeName, _resourceContext.Attributes[0].PublicAttributeName);
        }
    }
}
