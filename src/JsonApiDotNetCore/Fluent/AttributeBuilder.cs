using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System;
using System.Reflection;

namespace JsonApiDotNetCore.Fluent
{
    public class AttributeBuilder<TResource> : BaseBuilder<TResource>
    {        
        private readonly PropertyInfo _property;        
        private AttrAttribute _attribute;

        private string _publicName;
        private AttrCapabilities? _capabilities;

        public AttributeBuilder(ResourceContext resourceContext, IJsonApiOptions options, PropertyInfo property): base(resourceContext, options)
        {     
            _property = property;            
        }

        protected string GetPublicNameOrConvention()
        {
            return !string.IsNullOrWhiteSpace(_publicName) ? _publicName : FormatPropertyName(_property);
        }

        protected AttrCapabilities GetCapabilitiesOrDefault()
        {
            return _capabilities.HasValue ? _capabilities.Value : _options.DefaultAttrCapabilities;
        }

        public override void Build()
        {
            _attribute = new AttrAttribute(GetPublicNameOrConvention(),
                                           GetCapabilitiesOrDefault());

            _attribute.PropertyInfo = _property;
            
            _resourceContext.Attributes = CombineAnnotations<AttrAttribute>(_attribute, _resourceContext.Attributes, AttrAttributeComparer.Instance);            
        }

        public AttributeBuilder<TResource> PublicName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Exposed name cannot be empty or contain only whitespace.", nameof(name));
            }

            _publicName = name;

            this.Build();

            return this;
        }

        public AttributeBuilder<TResource> Capabilites(AttrCapabilities capabilities)
        {
            _capabilities = capabilities;

            this.Build();

            return this;
        }       
    }
}
