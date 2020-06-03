using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_PublicName : HasOneBuilderSpecificationBase
    {        
        protected override async Task Given()
        {
            await base.Given();

            SetupHasOneBuilderWithUnAnnotatedProperty();

            _hasOneBuilder.Build();
        }

        protected override async Task When()
        {
            await base.When();

            _hasOneBuilder.PublicName("product-image");
        }

        [Then]
        public void It_Should_Override_PublicRelationshipName_Default_Format()
        {
            Assert.Equal("product-image", _resourceContext.Relationships[0].PublicRelationshipName);
        }
    }
}
