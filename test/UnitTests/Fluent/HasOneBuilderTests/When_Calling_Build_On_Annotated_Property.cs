using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_Build_On_Annotated_Property : HasOneBuilderSpecificationBase
    {        
        protected override async Task Given()
        {
            await base.Given();

            SetupHasOneBuilderWithAnnotatedProperty();           
        }

        protected override async Task When()
        {
            await base.When();

            _hasOneBuilder.Build();
        }
        
        //[Then]
        public void It_Should_Override_HasOne_On_ResourceContext()
        {            
            Assert.Single(_resourceContext.Relationships);
            Assert.IsType<HasOneAttribute>(_resourceContext.Relationships[0]);
        }

        //[Then]
        public void It_Should_Override_PublicRelationshipName()
        {
            Assert.Equal("image", _resourceContext.Relationships[0].PublicRelationshipName);
        }        
    }
}
