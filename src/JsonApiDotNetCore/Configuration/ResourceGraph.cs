using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    [PublicAPI]
    public class ResourceGraph : IResourceGraph
    {
        private static readonly Type ProxyTargetAccessorType = Type.GetType("Castle.DynamicProxy.IProxyTargetAccessor, Castle.Core");
        private readonly IReadOnlyCollection<ResourceContext> _resources;

        public ResourceGraph(IReadOnlyCollection<ResourceContext> resources)
        {
            ArgumentGuard.NotNull(resources, nameof(resources));

            _resources = resources;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceContext> GetResourceContexts()
        {
            return _resources;
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext(string resourceName)
        {
            ArgumentGuard.NotNullNorEmpty(resourceName, nameof(resourceName));

            return _resources.SingleOrDefault(resourceContext => resourceContext.PublicName == resourceName);
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            return IsLazyLoadingProxyForResourceType(resourceType)
                ? _resources.SingleOrDefault(resourceContext => resourceContext.ResourceType == resourceType.BaseType)
                : _resources.SingleOrDefault(resourceContext => resourceContext.ResourceType == resourceType);
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext<TResource>()
            where TResource : class, IIdentifiable
        {
            return GetResourceContext(typeof(TResource));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, dynamic>> selector = null)
            where TResource : class, IIdentifiable
        {
            return Getter(selector);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, dynamic>> selector = null)
            where TResource : class, IIdentifiable
        {
            return Getter(selector, FieldFilterType.Attribute).Cast<AttrAttribute>().ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, dynamic>> selector = null)
            where TResource : class, IIdentifiable
        {
            return Getter(selector, FieldFilterType.Relationship).Cast<RelationshipAttribute>().ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceFieldAttribute> GetFields(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            return GetResourceContext(type).Fields;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            return GetResourceContext(type).Attributes;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            return GetResourceContext(type).Relationships;
        }

        /// <inheritdoc />
        public RelationshipAttribute GetInverseRelationship(RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));

            if (relationship.InverseNavigationProperty == null)
            {
                return null;
            }

            return GetResourceContext(relationship.RightType).Relationships
                .SingleOrDefault(nextRelationship => nextRelationship.Property == relationship.InverseNavigationProperty);
        }

        private IReadOnlyCollection<ResourceFieldAttribute> Getter<TResource>(Expression<Func<TResource, dynamic>> selector = null,
            FieldFilterType type = FieldFilterType.None)
            where TResource : class, IIdentifiable
        {
            IReadOnlyCollection<ResourceFieldAttribute> available;

            if (type == FieldFilterType.Attribute)
            {
                available = GetResourceContext(typeof(TResource)).Attributes;
            }
            else if (type == FieldFilterType.Relationship)
            {
                available = GetResourceContext(typeof(TResource)).Relationships;
            }
            else
            {
                available = GetResourceContext(typeof(TResource)).Fields;
            }

            if (selector == null)
            {
                return available;
            }

            var targeted = new List<ResourceFieldAttribute>();

            Expression selectorBody = RemoveConvert(selector.Body);

            if (selectorBody is MemberExpression memberExpression)
            {
                // model => model.Field1
                try
                {
                    targeted.Add(available.Single(field => field.Property.Name == memberExpression.Member.Name));
                    return targeted;
                }
                catch (InvalidOperationException)
                {
                    ThrowNotExposedError(memberExpression.Member.Name, type);
                }
            }

            if (selectorBody is NewExpression newExpression)
            {
                // model => new { model.Field1, model.Field2 }
                string memberName = null;

                try
                {
                    if (newExpression.Members == null)
                    {
                        return targeted;
                    }

                    foreach (MemberInfo member in newExpression.Members)
                    {
                        memberName = member.Name;
                        targeted.Add(available.Single(field => field.Property.Name == memberName));
                    }

                    return targeted;
                }
                catch (InvalidOperationException)
                {
                    ThrowNotExposedError(memberName, type);
                }
            }

            throw new ArgumentException($"The expression '{selector}' should select a single property or select multiple properties into an anonymous type. " +
                "For example: 'article => article.Title' or 'article => new { article.Title, article.PageCount }'.");
        }

        private bool IsLazyLoadingProxyForResourceType(Type resourceType)
        {
            return ProxyTargetAccessorType?.IsAssignableFrom(resourceType) ?? false;
        }

        private static Expression RemoveConvert(Expression expression)
        {
            Expression innerExpression = expression;

            while (true)
            {
                if (innerExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                {
                    innerExpression = unaryExpression.Operand;
                }
                else
                {
                    return innerExpression;
                }
            }
        }

        private void ThrowNotExposedError(string memberName, FieldFilterType type)
        {
            throw new ArgumentException($"{memberName} is not a JSON:API exposed {type:g}.");
        }

        private enum FieldFilterType
        {
            None,
            Attribute,
            Relationship
        }
    }
}
