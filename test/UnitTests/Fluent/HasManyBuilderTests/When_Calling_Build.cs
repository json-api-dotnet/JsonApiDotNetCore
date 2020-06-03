using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasManyBuilderTests
{
    public class When_Calling_Build: HasManyBuilderSpecificationBase
    {
        HasManyBuilder<UnAnnotatedProduct> _hasManyBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceContext.ResourceType = typeof(UnAnnotatedProduct);

            _property = typeof(UnAnnotatedProduct).GetProperty("Tags");

            _hasManyBuilder = new HasManyBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property);
        }

        protected override async Task When()
        {
            await base.When();

            _hasManyBuilder.Build();
        }

        [Then]
        public void It_Should_Add_HasMany_To_ResourceContext()
        {            
            Assert.Single(_resourceContext.Relationships);
            Assert.IsType<HasManyAttribute>(_resourceContext.Relationships[0]);
        }

        [Then]
        public void It_Should_Associate_HasMany_With_Property()
        {
            Assert.Equal(_property, _resourceContext.Relationships[0].PropertyInfo);
        }

        [Then]
        public void It_Should_Format_PublicRelationshipName_According_To_NamingStrategy()
        {
            Assert.Equal("tags", _resourceContext.Relationships[0].PublicRelationshipName);
        }

        [Then]
        public void It_Should_Assign_Resource_As_LeftType()
        {
            Assert.Equal(typeof(UnAnnotatedProduct), _resourceContext.Relationships[0].LeftType);
        }

        [Then]
        public void It_Should_Assign_Relation_As_RightType()
        {
            Assert.Equal(typeof(Tag), _resourceContext.Relationships[0].RightType);
        }
    }
}
