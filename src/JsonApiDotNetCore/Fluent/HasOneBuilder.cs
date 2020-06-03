using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Fluent
{
    public class HasOneBuilder<TResource>: RelationshipBuilder<TResource>
    {                
        private HasOneAttribute _attribute;
        
        public HasOneBuilder(ResourceContext resourceContext, IJsonApiOptions options, PropertyInfo property) : base(resourceContext, options, property)
        {            
            
        }

        public override void Build()
        {            
            _attribute = new HasOneAttribute(GetPublicNameOrConvention(),
                                             GetRelationshipLinksOrDefault(),
                                             GetCanIncludeOrDefault(),
                                             GetForeignKeyPropertyOrDefault(),
                                             GetInverseNavigationPropertyOrDefault());

            _attribute.PropertyInfo = _property;            
            _attribute.RightType = GetRelationshipType(_attribute, _property);
            _attribute.LeftType = _resourceContext.ResourceType; 
            
            _resourceContext.Relationships = CombineAnnotations<RelationshipAttribute>(_attribute, _resourceContext.Relationships, RelationshipAttributeComparer.Instance);
        }

        public new HasOneBuilder<TResource> PublicName(string name)
        {            
            return (HasOneBuilder<TResource>)base.PublicName(name);
        }

        public new HasOneBuilder<TResource> CanInclude(bool include)
        {
            return (HasOneBuilder<TResource>)base.CanInclude(include);
        }

        public new HasOneBuilder<TResource> Links(Link relationshipLinks)
        {
            return (HasOneBuilder<TResource>)base.Links(relationshipLinks);
        }

        public new HasOneBuilder<TResource> WithForeignKey(Expression<Func<TResource, object>> memberExpression)
        {
            return (HasOneBuilder<TResource>)base.WithForeignKey(memberExpression);
        }

        public new HasOneBuilder<TResource> InverseNavigation(Expression<Func<TResource, object>> memberExpression)
        {
            return (HasOneBuilder<TResource>)base.InverseNavigation(memberExpression);
        }
    }
}
