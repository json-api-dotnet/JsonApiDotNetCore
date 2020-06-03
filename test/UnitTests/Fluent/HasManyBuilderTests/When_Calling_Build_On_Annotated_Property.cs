using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyBuilderTests
{
    public sealed class When_Calling_Build_On_Annotated_Property : HasManyBuilderSpecificationBase
    {
        HasManyBuilder<AnnotatedProduct> _hasManyBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(AnnotatedProduct);

            _property = typeof(AnnotatedProduct).GetProperty("Tags");

            _hasManyAttribute = _property.GetCustomAttribute(typeof(HasManyAttribute)) as HasManyAttribute;
            _hasManyAttribute.PropertyInfo = _property;
            _resourceContext.Relationships.Add(_hasManyAttribute);

            _hasManyBuilder = new HasManyBuilder<AnnotatedProduct>(_resourceContext, _options, _property);
        }

        protected override async Task When()
        {
            await base.When();

            _hasManyBuilder.Build();
        }

        [Then]
        public void It_Should_Override_HasMany_On_ResourceContext()
        {            
            Assert.Single(_resourceContext.Relationships);
            Assert.IsType<HasManyAttribute>(_resourceContext.Relationships[0]);
        }
        
        [Then]
        public void It_Should_Override_PublicRelationshipName()
        {
            Assert.Equal("tags", _resourceContext.Relationships[0].PublicRelationshipName);
        }        
    }
}
