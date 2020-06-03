using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JsonApiDotNetCore.Fluent
{
    public class HasManyBuilder<TResource>: RelationshipBuilder<TResource>
    {
        private const Link RelationshipLinksDefault = Link.All;
        private HasManyAttribute _attribute;

        public HasManyBuilder(ResourceContext resourceContext, IJsonApiOptions options, PropertyInfo property) : base(resourceContext, options, property)
        {            
            
        }

        public override void Build()
        {                        
            _attribute = new HasManyAttribute(GetPublicNameOrConvention(),
                                              GetRelationshipLinksOrDefault(),
                                              GetCanIncludeOrDefault(),                                              
                                              GetInverseNavigationPropertyOrDefault());
                                              
            _attribute.PropertyInfo = _property;            
            _attribute.RightType = GetRelationshipType(_attribute, _property);
            _attribute.LeftType = _resourceContext.ResourceType;

            _resourceContext.Relationships = CombineAnnotations<RelationshipAttribute>(_attribute, _resourceContext.Relationships, RelationshipAttributeComparer.Instance);            
        }

        protected override Link GetRelationshipLinksOrDefault()
        {
            return _relationshipLinks.HasValue ? _relationshipLinks.Value : RelationshipLinksDefault;
        }

        public new HasManyBuilder<TResource> PublicName(string name)
        {            
            return (HasManyBuilder<TResource>)base.PublicName(name);
        }

        public new HasManyBuilder<TResource> CanInclude(bool include)
        {
            return (HasManyBuilder<TResource>)base.CanInclude(include);
        }

        public new HasManyBuilder<TResource> Links(Link relationshipLinks)
        {
            return (HasManyBuilder<TResource>)base.Links(relationshipLinks);
        }

        public new HasManyBuilder<TResource> InverseNavigation(Expression<Func<TResource, object>> memberExpression)
        {
            return (HasManyBuilder<TResource>)base.InverseNavigation(memberExpression);
        }
    }
}
