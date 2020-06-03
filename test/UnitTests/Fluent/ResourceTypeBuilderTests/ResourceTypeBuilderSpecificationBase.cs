using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;

namespace UnitTests.Fluent.ResourceTypeBuilderTests
{
    public abstract class ResourceTypeBuilderSpecificationBase: SpecificationBase
    {
        protected JsonApiOptions _options;
        protected IResourceGraphBuilder _resourceGraphBuilder;
        protected ResourceContext _resourceContext;

        protected override async Task Given()
        {
            await base.Given();

            _options = new JsonApiOptions();

            _resourceGraphBuilder = new ResourceGraphBuilder(_options, NullLoggerFactory.Instance);            
        }
    }
}
