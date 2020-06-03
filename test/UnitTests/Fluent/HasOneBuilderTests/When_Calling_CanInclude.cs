using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_CanInclude : HasOneBuilderSpecificationBase
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

            _hasOneBuilder.CanInclude(false);
        }

        [Then]
        public void It_Should_Configure_CanInclude()
        {            
            Assert.False(((HasOneAttribute)_resourceContext.Relationships[0]).CanInclude);            
        }        
    }
}
