using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JsonApiDotNetCore.Fluent
{
    public class HasManyThroughBuilder<TResource> : RelationshipBuilder<TResource>
    {
        private const Link RelationshipLinksDefault = Link.All;
        private readonly PropertyInfo _throughProperty;        
        private HasManyThroughAttribute _attribute;

        public HasManyThroughBuilder(ResourceContext resourceContext, IJsonApiOptions options, PropertyInfo property, PropertyInfo throughProperty): base(resourceContext, options, property)
        {                        
            _throughProperty = throughProperty;
        }

        public override void Build()
        {
            _attribute = new HasManyThroughAttribute(_throughProperty.Name);

            _attribute = new HasManyThroughAttribute(GetPublicNameOrConvention(),
                                                     _throughProperty.Name,
                                                     GetRelationshipLinksOrDefault(),
                                                     GetCanIncludeOrDefault());
            
            _attribute.PropertyInfo = _property;
            _attribute.ThroughProperty = _throughProperty;            
            _attribute.RightType = GetRelationshipType(_attribute, _property);
            _attribute.LeftType = _resourceContext.ResourceType;

            Type entityType = _resourceContext.ResourceType;

            if (_throughProperty == null)
            {
                throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{entityType}.{_attribute.PropertyInfo.Name}': Resource does not contain a property named '{_attribute.ThroughPropertyName}'.");
            }

            Type throughType = ResourceGraphBuilder.TryGetThroughType(_throughProperty);

            if (throughType == null)
            {
                throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{entityType}.{_attribute.PropertyInfo.Name}': Referenced property '{_throughProperty.Name}' does not implement 'ICollection<T>'.");
            }

            // ICollection<ArticleTag>
            _attribute.ThroughProperty = _throughProperty;

            // ArticleTag
            _attribute.ThroughType = throughType;

            var throughProperties = throughType.GetProperties();

            // ArticleTag.Article
            _attribute.LeftProperty = throughProperties.SingleOrDefault(x => x.PropertyType == entityType)
                ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {entityType}");

            // ArticleTag.ArticleId
            var leftIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(_attribute.LeftProperty.Name);
            _attribute.LeftIdProperty = throughProperties.SingleOrDefault(x => x.Name == leftIdPropertyName)
                ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {entityType} with name {leftIdPropertyName}");

            // ArticleTag.Tag
            _attribute.RightProperty = throughProperties.SingleOrDefault(x => x.PropertyType == _attribute.RightType)
                ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {_attribute.RightType}");

            // ArticleTag.TagId
            var rightIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(_attribute.RightProperty.Name);
            _attribute.RightIdProperty = throughProperties.SingleOrDefault(x => x.Name == rightIdPropertyName)
                ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {_attribute.RightType} with name {rightIdPropertyName}");

            _resourceContext.Relationships = CombineAnnotations<RelationshipAttribute>(_attribute, _resourceContext.Relationships, RelationshipAttributeComparer.Instance);
        }

        protected override Link GetRelationshipLinksOrDefault()
        {
            return _relationshipLinks.HasValue ? _relationshipLinks.Value : RelationshipLinksDefault;
        }

        public new HasManyThroughBuilder<TResource> PublicName(string name)
        {            
            return (HasManyThroughBuilder<TResource>)base.PublicName(name);
        }

        public new HasManyThroughBuilder<TResource> CanInclude(bool include)
        {
            return (HasManyThroughBuilder<TResource>)base.CanInclude(include);
        }

        public new HasManyThroughBuilder<TResource> Links(Link relationshipLinks)
        {
            return (HasManyThroughBuilder<TResource>)base.Links(relationshipLinks);
        }
    }
}
