using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;

namespace UnitTests.Fluent.EagerLoadBuilderTests
{
    public abstract class EagerLoadBuilderSpecificationBase: SpecificationBase
    {
        protected ResourceContext _resourceContext;
        protected JsonApiOptions _options;
        protected PropertyInfo _property;
        protected EagerLoadAttribute _eagerLoadAttribute;        

        protected override async Task Given()
        {
            await base.Given();

            _options = new JsonApiOptions();

            _resourceContext = new ResourceContext();
            _resourceContext.EagerLoads = new List<EagerLoadAttribute>();
        }
    }
}
