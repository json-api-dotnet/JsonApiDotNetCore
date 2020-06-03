using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public abstract class HasOneBuilderSpecificationBase : SpecificationBase
    {
        protected ResourceContext _resourceContext;
        protected JsonApiOptions _options;
        protected PropertyInfo _property;
        protected HasOneAttribute _hasOneAttribute;
        protected HasOneBuilder<UnAnnotatedProduct> _hasOneBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _options = new JsonApiOptions();

            _resourceContext = new ResourceContext();            
            _resourceContext.Relationships = new List<RelationshipAttribute>();
        }

        protected void SetupHasOneBuilderWithUnAnnotatedProperty()
        {
            _resourceContext.ResourceType = typeof(UnAnnotatedProduct);

            _property = typeof(UnAnnotatedProduct).GetProperty("Image");

            _hasOneBuilder = new HasOneBuilder<UnAnnotatedProduct>(_resourceContext, _options, _property);
        }

        protected void SetupHasOneBuilderWithAnnotatedProperty()
        {
            _resourceContext.ResourceType = typeof(AnnotatedProduct);

            _property = typeof(AnnotatedProduct).GetProperty("Image");

            _hasOneAttribute = _property.GetCustomAttribute(typeof(HasOneAttribute)) as HasOneAttribute;
            _hasOneAttribute.PropertyInfo = _property;
            _resourceContext.Relationships.Add(_hasOneAttribute);

            //_hasOneBuilder = new HasOneBuilder(_resourceContext, _options, _property);
        }
    }
}
