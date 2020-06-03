using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyThroughBuilderTests
{
    public sealed class When_Calling_Build: HasManyThroughBuilderSpecificationBase
    {
        HasManyThroughBuilder<UnAnnotatedProduct> _hasManyThroughBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(UnAnnotatedProduct);

            _property = typeof(UnAnnotatedProduct).GetProperty("Categories");

            _throughProperty = typeof(UnAnnotatedProduct).GetProperty("ProductCategories");

            _hasManyThroughBuilder = new HasManyThroughBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property, _throughProperty);            
        }

        protected override async Task When()
        {
            await base.When();

            _hasManyThroughBuilder.Build();
        }

        [Then]
        public void It_Should_Add_HasManyThrough_To_ResourceContext()
        {            
            Assert.Single(_resourceContext.Relationships);
            Assert.IsType<HasManyThroughAttribute>(_resourceContext.Relationships[0]);
        }

        [Then]
        public void It_Should_Associate_HasManyThrough_With_Property()
        {
            Assert.Equal(_property, _resourceContext.Relationships[0].PropertyInfo);
        }

        [Then]
        public void It_Should_Associate_HasManyThrough_With_ThroughProperty()
        {
            Assert.Equal(_throughProperty, ((HasManyThroughAttribute)_resourceContext.Relationships[0]).ThroughProperty);
        }

        [Then]
        public void It_Should_Find_HasManyThrough_With_Through_Type()
        {
            Assert.Equal(typeof(UnAnnotatedProductCategories), ((HasManyThroughAttribute)_resourceContext.Relationships[0]).ThroughType);
        }

        [Then]
        public void It_Should_Format_PublicRelationshipName_According_To_NamingStrategy()
        {
            Assert.Equal("categories", _resourceContext.Relationships[0].PublicRelationshipName);
        }

        [Then]
        public void It_Should_Assign_Resource_As_LeftType()
        {
            Assert.Equal(typeof(UnAnnotatedProduct), _resourceContext.Relationships[0].LeftType);
        }

        [Then]
        public void It_Should_Assign_Relation_As_RightType()
        {
            Assert.Equal(typeof(Category), _resourceContext.Relationships[0].RightType);
        }
    }
}
