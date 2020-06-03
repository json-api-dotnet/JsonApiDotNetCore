using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Fluent
{
    public class ResourceTypeBuilder<TResource>
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceGraphBuilder _resourceGraphBuilder;
        private readonly ResourceContext _resourceContext;

        public ResourceTypeBuilder(IResourceGraphBuilder resourceGraphBuilder, IJsonApiOptions options)
        {
            _resourceGraphBuilder = resourceGraphBuilder;
            _options = options;

            _resourceContext = _resourceGraphBuilder.GetResourceContext(typeof(TResource));
        }

        public ResourceTypeBuilder<TResource> ResourceName(string name)
        {
            _resourceContext.ResourceName = name;
            
            return this;
        }

        public ResourceTypeBuilder<TResource> Links(Link topLevelLinksOptions, Link resourceLinksOptions, Link relationshipLinksOptions)
        {
            LinksBuilder<TResource> linksBuilder = new LinksBuilder<TResource>(_resourceContext, _options, topLevelLinksOptions, resourceLinksOptions, relationshipLinksOptions);
            linksBuilder.Build();

            return this;
        }

        public AttributeBuilder<TResource> Attribute(Expression<Func<TResource, object>> memberExpression)
        {
            PropertyInfo property = TypeHelper.ParseNavigationExpression(memberExpression);

            AttributeBuilder<TResource> attributeBuilder = new AttributeBuilder<TResource>(_resourceContext, _options, property);
            attributeBuilder.Build();

            return attributeBuilder;
        }

        public HasOneBuilder<TResource> HasOne(Expression<Func<TResource, object>> memberExpression)
        {
            PropertyInfo property = TypeHelper.ParseNavigationExpression(memberExpression);

            HasOneBuilder<TResource> hasOneBuilder = new HasOneBuilder<TResource>(_resourceContext, _options, property);
            hasOneBuilder.Build();

            return hasOneBuilder;
        }

        public HasManyBuilder<TResource> HasMany(Expression<Func<TResource, object>> memberExpression)
        {
            PropertyInfo property = TypeHelper.ParseNavigationExpression(memberExpression);

            HasManyBuilder<TResource> hasManyBuilder = new HasManyBuilder<TResource>(_resourceContext, _options, property);
            hasManyBuilder.Build();

            return hasManyBuilder;
        }

        public HasManyThroughBuilder<TResource> HasManyThrough(Expression<Func<TResource, object>> memberExpression, Expression<Func<TResource, object>> throughExpression)
        {
            PropertyInfo property = TypeHelper.ParseNavigationExpression(memberExpression);
            PropertyInfo throughProperty = TypeHelper.ParseNavigationExpression(throughExpression);

            HasManyThroughBuilder<TResource> hasManyThroughBuilder = new HasManyThroughBuilder<TResource>(_resourceContext, _options, property, throughProperty);
            hasManyThroughBuilder.Build();
            
            return hasManyThroughBuilder;
        }

        public EagerLoadBuilder<TResource> EagerLoad(Expression<Func<TResource, object>> memberExpression)
        {
            PropertyInfo property = TypeHelper.ParseNavigationExpression(memberExpression);

            EagerLoadBuilder<TResource> eagerLoadBuilder = new EagerLoadBuilder<TResource>(_resourceContext, _options, property);
            eagerLoadBuilder.Build();

            return eagerLoadBuilder;
        }        
    }
}
