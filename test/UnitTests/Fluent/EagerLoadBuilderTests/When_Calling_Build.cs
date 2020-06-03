using JsonApiDotNetCore.Fluent;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.EagerLoadBuilderTests
{
    public sealed class When_Calling_Build: EagerLoadBuilderSpecificationBase
    {
        EagerLoadBuilder<UnAnnotatedProduct> _eagerLoadBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _property = typeof(UnAnnotatedProduct).GetProperty("UnitPrice");

            _eagerLoadBuilder = new EagerLoadBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property);
        }

        protected override async Task When()
        {
            await base.When();

            _eagerLoadBuilder.Build();
        }

        [Then]
        public void It_Should_Add_EagerLoad_To_ResourceContext()
        {            
            Assert.Single(_resourceContext.EagerLoads);
        }

        [Then]
        public void It_Should_Associate_EagerLoad_With_Property()
        {
            Assert.Equal(_property, _resourceContext.EagerLoads[0].Property);
        }        
    }
}
