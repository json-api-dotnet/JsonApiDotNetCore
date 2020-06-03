using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;

namespace UnitTests.Fluent.HasManyBuilderTests
{
    public abstract class HasManyBuilderSpecificationBase : SpecificationBase
    {
        protected ResourceContext _resourceContext;
        protected JsonApiOptions _options;
        protected PropertyInfo _property;
        protected HasManyAttribute _hasManyAttribute;
        
        protected override async Task Given()
        {
            await base.Given();

            _options = new JsonApiOptions();

            _resourceContext = new ResourceContext();            
            _resourceContext.Relationships = new List<RelationshipAttribute>();
        }        
    }
}
