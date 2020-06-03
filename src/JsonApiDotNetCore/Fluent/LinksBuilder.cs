using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Fluent
{
    public class LinksBuilder<TResource> : BaseBuilder<TResource>
    {
        private LinksAttribute _attribute;
        
        public LinksBuilder(ResourceContext resourceContext, IJsonApiOptions options, Link topLevelLinksOptions, Link resourceLinksOptions, Link relationshipLinksOptions): base(resourceContext, options)
        {
            _attribute = new LinksAttribute(topLevelLinksOptions,
                                            resourceLinksOptions,
                                            relationshipLinksOptions);            
        }

        public override void Build()
        {         
            _resourceContext.TopLevelLinks = _attribute.TopLevelLinks;
            _resourceContext.ResourceLinks = _attribute.ResourceLinks;
            _resourceContext.RelationshipLinks = _attribute.RelationshipLinks;
        }
    }
}
