using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Reflection;

namespace JsonApiDotNetCore.Fluent
{
    public class EagerLoadBuilder<TResource>: BaseBuilder<TResource>
    {        
        private readonly PropertyInfo _property;
        private EagerLoadAttribute _attribute;

        public EagerLoadBuilder(ResourceContext resourceContext, IJsonApiOptions options, PropertyInfo property): base(resourceContext, options)
        {            
            _property = property;
        }

        public override void Build()
        {
            _attribute = new EagerLoadAttribute();

            _attribute.Property = _property;

            _resourceContext.EagerLoads = CombineAnnotations<EagerLoadAttribute>(_attribute, _resourceContext.EagerLoads, EagerLoadAttributeComparer.Instance);
        }
    }
}
