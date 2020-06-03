using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_Links : HasOneBuilderSpecificationBase
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

            _hasOneBuilder.Links(Link.Self | Link.Related);
        }

        [Then]
        public void It_Should_Configure_Links()
        {            
            Assert.Equal((Link.Self | Link.Related), ((HasOneAttribute)_resourceContext.Relationships[0]).RelationshipLinks);            
        }        
    }
}
