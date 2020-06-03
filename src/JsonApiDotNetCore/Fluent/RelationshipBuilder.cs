using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Fluent
{
    public abstract class RelationshipBuilder<TResource>: BaseBuilder<TResource>        
    {
        private const Link RelationshipLinksDefault = Link.NotConfigured;
        private const bool CanIncludeDefault = true;

        protected PropertyInfo _property;
        protected PropertyInfo _foreignKeyProperty;
        protected PropertyInfo _inverseNavigationProperty;
        protected string _publicName;
        protected Link? _relationshipLinks;
        protected bool? _canInclude;
        
        public RelationshipBuilder(ResourceContext resourceContext, IJsonApiOptions options, PropertyInfo property): base(resourceContext, options)
        {            
            _property = property;     
        }

        protected Type GetRelationshipType(RelationshipAttribute relation, PropertyInfo prop)
        {
            return relation is HasOneAttribute ? prop.PropertyType : prop.PropertyType.GetGenericArguments()[0];
        }

        protected string GetPublicNameOrConvention()
        {
            return !string.IsNullOrWhiteSpace(_publicName) ? _publicName : FormatPropertyName(_property);            
        }
        
        protected virtual Link GetRelationshipLinksOrDefault()
        {
            return _relationshipLinks.HasValue ? _relationshipLinks.Value : RelationshipLinksDefault;            
        }

        protected bool GetCanIncludeOrDefault()
        {
            return _canInclude.HasValue ? _canInclude.Value : CanIncludeDefault;            
        }

        protected string GetForeignKeyPropertyOrDefault()
        {
            return _foreignKeyProperty != null ? _foreignKeyProperty.Name : null;
        }

        protected string GetInverseNavigationPropertyOrDefault()
        {
            return _inverseNavigationProperty != null ? _inverseNavigationProperty.Name : null;
        }

        protected RelationshipBuilder<TResource> PublicName(string name)
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

        protected RelationshipBuilder<TResource> CanInclude(bool include)
        {
            _canInclude = include;

            this.Build();

            return this;
        }

        protected RelationshipBuilder<TResource> Links(Link relationshipLinks)
        {
            _relationshipLinks = relationshipLinks;

            this.Build();

            return this;
        }

        protected RelationshipBuilder<TResource> WithForeignKey(Expression<Func<TResource, object>> memberExpression)
        {
            _foreignKeyProperty = TypeHelper.ParseNavigationExpression(memberExpression);

            this.Build();

            return this;
        }

        protected RelationshipBuilder<TResource> InverseNavigation(Expression<Func<TResource, object>> memberExpression)
        {
            _inverseNavigationProperty = TypeHelper.ParseNavigationExpression(memberExpression);

            this.Build();

            return this;
        }
    }
}
