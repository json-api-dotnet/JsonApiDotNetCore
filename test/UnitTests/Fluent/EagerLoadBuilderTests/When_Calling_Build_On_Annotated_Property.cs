using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Models;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.EagerLoadBuilderTests
{
    public sealed class When_Calling_Build_On_Annotated_Property : EagerLoadBuilderSpecificationBase
    {
        EagerLoadBuilder<AnnotatedProduct> _eagerLoadBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _property = typeof(AnnotatedProduct).GetProperty("UnitPrice");

            _eagerLoadAttribute = _property.GetCustomAttribute(typeof(EagerLoadAttribute)) as EagerLoadAttribute;
            _eagerLoadAttribute.Property = _property;
            _resourceContext.EagerLoads.Add(_eagerLoadAttribute);

            _eagerLoadBuilder = new EagerLoadBuilder<AnnotatedProduct>(_resourceContext, _options, _property);
        }

        protected override async Task When()
        {
            await base.When();

            _eagerLoadBuilder.Build();
        }

        [Then]
        public void It_Should_Override_EagerLoad_On_ResourceContext()
        {            
            Assert.Single(_resourceContext.EagerLoads);
        }                
    }
}
