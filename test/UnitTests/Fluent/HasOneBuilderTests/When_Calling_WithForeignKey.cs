using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using UnitTests.Specifications;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_WithForeignKey : HasOneBuilderSpecificationBase
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

            _hasOneBuilder.WithForeignKey(x => x.ImageIdentifier);
        }

        [Then]
        public void It_Should_Override_Convention()
        {            
            Assert.NotEqual("ImageId", ((HasOneAttribute)_resourceContext.Relationships[0]).IdentifiablePropertyName);
            Assert.Equal("ImageIdentifier", ((HasOneAttribute)_resourceContext.Relationships[0]).IdentifiablePropertyName);
        }        
    }
}
