using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyThroughBuilderTests
{
    public sealed class When_Calling_Build_On_Annotated_Property : HasManyThroughBuilderSpecificationBase
    {
        HasManyThroughBuilder<AnnotatedProduct> _hasManyThroughBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(AnnotatedProduct);

            _property = typeof(AnnotatedProduct).GetProperty("Categories");

            _throughProperty = typeof(AnnotatedProduct).GetProperty("ProductCategories");

            _hasManyThroughAttribute = _property.GetCustomAttribute(typeof(HasManyThroughAttribute)) as HasManyThroughAttribute;
            _hasManyThroughAttribute.PropertyInfo = _property;
            _resourceContext.Relationships.Add(_hasManyThroughAttribute);

            _hasManyThroughBuilder =  new HasManyThroughBuilder<AnnotatedProduct>(_resourceContext, _options, _property, _throughProperty);            
        }

        protected override async Task When()
        {
            await base.When();

            _hasManyThroughBuilder.Build();
        }

        [Then]
        public void It_Should_Override_HasManyThrough_On_ResourceContext()
        {            
            Assert.Single(_resourceContext.Relationships);
            Assert.IsType<HasManyThroughAttribute>(_resourceContext.Relationships[0]);
        }

        [Then]
        public void It_Should_Override_PublicRelationshipName()
        {
            Assert.Equal("categories", _resourceContext.Relationships[0].PublicRelationshipName);
        }        
    }
}
