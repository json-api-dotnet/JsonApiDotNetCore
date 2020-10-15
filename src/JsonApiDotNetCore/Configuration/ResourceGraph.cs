using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public class ResourceGraph : IResourceGraph
    {
        private readonly IReadOnlyCollection<ResourceContext> _resources;
        private static readonly Type _proxyTargetAccessorType = Type.GetType("Castle.DynamicProxy.IProxyTargetAccessor, Castle.Core");

        public ResourceGraph(IReadOnlyCollection<ResourceContext> resources)
        {
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceContext> GetResourceContexts() => _resources;

        /// <inheritdoc />
        public ResourceContext GetResourceContext(string resourceName)
        {
            if (resourceName == null) throw new ArgumentNullException(nameof(resourceName));

            return _resources.SingleOrDefault(e => e.PublicName == resourceName);
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            return IsLazyLoadingProxyForResourceType(resourceType)
                ? _resources.SingleOrDefault(e => e.ResourceType == resourceType.BaseType)
                : _resources.SingleOrDefault(e => e.ResourceType == resourceType);
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext<TResource>() where TResource : class, IIdentifiable
            => GetResourceContext(typeof(TResource));

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, dynamic>> selector = null) where TResource : class, IIdentifiable
        {
            return Getter(selector);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, dynamic>> selector = null) where TResource : class, IIdentifiable
        {
            return Getter(selector, FieldFilterType.Attribute).Cast<AttrAttribute>().ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, dynamic>> selector = null) where TResource : class, IIdentifiable
        {
            return Getter(selector, FieldFilterType.Relationship).Cast<RelationshipAttribute>().ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceFieldAttribute> GetFields(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return GetResourceContext(type).Fields;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return GetResourceContext(type).Attributes;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return GetResourceContext(type).Relationships;
        }

        /// <inheritdoc />
        public RelationshipAttribute GetInverseRelationship(RelationshipAttribute relationship)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));

            if (relationship.InverseNavigationProperty == null)
            {
                return null;
            }

            return GetResourceContext(relationship.RightType)
                .Relationships
                .SingleOrDefault(r => r.Property == relationship.InverseNavigationProperty);
        }

        private IReadOnlyCollection<ResourceFieldAttribute> Getter<TResource>(Expression<Func<TResource, dynamic>> selector = null, FieldFilterType type = FieldFilterType.None) where TResource : class, IIdentifiable
        {
            IReadOnlyCollection<ResourceFieldAttribute> available;
            if (type == FieldFilterType.Attribute)
                available = GetResourceContext(typeof(TResource)).Attributes;
            else if (type == FieldFilterType.Relationship)
                available = GetResourceContext(typeof(TResource)).Relationships;
            else
                available = GetResourceContext(typeof(TResource)).Fields;

            if (selector == null)
                return available;

            var targeted = new List<ResourceFieldAttribute>();

            var selectorBody = RemoveConvert(selector.Body);

            if (selectorBody is MemberExpression memberExpression)
            {   
                // model => model.Field1
                try
                {
                    targeted.Add(available.Single(f => f.Property.Name == memberExpression.Member.Name));
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
                        return targeted;

                    foreach (var member in newExpression.Members)
                    {
                        memberName = member.Name;
                        targeted.Add(available.Single(f => f.Property.Name == memberName));
                    }
                    return targeted;
                }
                catch (InvalidOperationException)
                {
                    ThrowNotExposedError(memberName, type);
                }
            }

            throw new ArgumentException(
                $"The expression '{selector}' should select a single property or select multiple properties into an anonymous type. " +
                "For example: 'article => article.Title' or 'article => new { article.Title, article.PageCount }'.");
        }

        private bool IsLazyLoadingProxyForResourceType(Type resourceType) =>
            _proxyTargetAccessorType?.IsAssignableFrom(resourceType) ?? false;

        private static Expression RemoveConvert(Expression expression)
            => expression is UnaryExpression unaryExpression
               && unaryExpression.NodeType == ExpressionType.Convert
                ? RemoveConvert(unaryExpression.Operand)
                : expression;

        private void ThrowNotExposedError(string memberName, FieldFilterType type)
        {
            throw new ArgumentException($"{memberName} is not an json:api exposed {type:g}.");
        }

        private enum FieldFilterType
        {
            None,
            Attribute,
            Relationship
        }
    }
}
