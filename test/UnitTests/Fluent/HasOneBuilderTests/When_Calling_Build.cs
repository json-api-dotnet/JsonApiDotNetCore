using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_Build: HasOneBuilderSpecificationBase
    {        
        protected override async Task Given()
        {
            await base.Given();

            SetupHasOneBuilderWithUnAnnotatedProperty();           
        }

        protected override async Task When()
        {
            await base.When();

            _hasOneBuilder.Build();
        }

        [Then]
        public void It_Should_Add_HasOne_To_ResourceContext()
        {            
            Assert.Single(_resourceContext.Relationships);
            Assert.IsType<HasOneAttribute>(_resourceContext.Relationships[0]);
        }

        [Then]
        public void It_Should_Associate_HasOne_With_Property()
        {
            Assert.Equal(_property, _resourceContext.Relationships[0].PropertyInfo);
        }

        [Then]
        public void It_Should_Format_PublicRelationshipName_According_To_NamingStrategy()
        {
            Assert.Equal("image", _resourceContext.Relationships[0].PublicRelationshipName);
        }

        [Then]
        public void It_Should_Assign_Resource_As_LeftType()
        {
            Assert.Equal(typeof(UnAnnotatedProduct), _resourceContext.Relationships[0].LeftType);
        }

        [Then]
        public void It_Should_Assign_Relation_As_RightType()
        {
            Assert.Equal(typeof(Image), _resourceContext.Relationships[0].RightType);
        }
    }
}
