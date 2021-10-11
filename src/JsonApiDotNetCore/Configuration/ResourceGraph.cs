#nullable disable

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
    public sealed class ResourceGraph : IResourceGraph
    {
        private static readonly Type ProxyTargetAccessorType = Type.GetType("Castle.DynamicProxy.IProxyTargetAccessor, Castle.Core");

        private readonly IReadOnlySet<ResourceType> _resourceTypeSet;
        private readonly Dictionary<Type, ResourceType> _resourceTypesByClrType = new();
        private readonly Dictionary<string, ResourceType> _resourceTypesByPublicName = new();

        public ResourceGraph(IReadOnlySet<ResourceType> resourceTypeSet)
        {
            ArgumentGuard.NotNull(resourceTypeSet, nameof(resourceTypeSet));

            _resourceTypeSet = resourceTypeSet;

            foreach (ResourceType resourceType in resourceTypeSet)
            {
                _resourceTypesByClrType.Add(resourceType.ClrType, resourceType);
                _resourceTypesByPublicName.Add(resourceType.PublicName, resourceType);
            }
        }

        /// <inheritdoc />
        public IReadOnlySet<ResourceType> GetResourceTypes()
        {
            return _resourceTypeSet;
        }

        /// <inheritdoc />
        public ResourceType GetResourceType(string publicName)
        {
            ResourceType resourceType = FindResourceType(publicName);

            if (resourceType == null)
            {
                throw new InvalidOperationException($"Resource type '{publicName}' does not exist.");
            }

            return resourceType;
        }

        /// <inheritdoc />
        public ResourceType FindResourceType(string publicName)
        {
            ArgumentGuard.NotNull(publicName, nameof(publicName));

            return _resourceTypesByPublicName.TryGetValue(publicName, out ResourceType resourceType) ? resourceType : null;
        }

        /// <inheritdoc />
        public ResourceType GetResourceType(Type resourceClrType)
        {
            ResourceType resourceType = FindResourceType(resourceClrType);

            if (resourceType == null)
            {
                throw new InvalidOperationException($"Resource of type '{resourceClrType.Name}' does not exist.");
            }

            return resourceType;
        }

        /// <inheritdoc />
        public ResourceType FindResourceType(Type resourceClrType)
        {
            ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

            Type typeToFind = IsLazyLoadingProxyForResourceType(resourceClrType) ? resourceClrType.BaseType : resourceClrType;
            return _resourceTypesByClrType.TryGetValue(typeToFind!, out ResourceType resourceType) ? resourceType : null;
        }

        private bool IsLazyLoadingProxyForResourceType(Type resourceClrType)
        {
            return ProxyTargetAccessorType?.IsAssignableFrom(resourceClrType) ?? false;
        }

        /// <inheritdoc />
        public ResourceType GetResourceType<TResource>()
            where TResource : class, IIdentifiable
        {
            return GetResourceType(typeof(TResource));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(selector, nameof(selector));

            return FilterFields<TResource, ResourceFieldAttribute>(selector);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(selector, nameof(selector));

            return FilterFields<TResource, AttrAttribute>(selector);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(selector, nameof(selector));

            return FilterFields<TResource, RelationshipAttribute>(selector);
        }

        private IReadOnlyCollection<TField> FilterFields<TResource, TField>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable
            where TField : ResourceFieldAttribute
        {
            IReadOnlyCollection<TField> source = GetFieldsOfType<TResource, TField>();
            var matches = new List<TField>();

            foreach (string memberName in ToMemberNames(selector))
            {
                TField matchingField = source.FirstOrDefault(field => field.Property.Name == memberName);

                if (matchingField == null)
                {
                    throw new ArgumentException($"Member '{memberName}' is not exposed as a JSON:API field.");
                }

                matches.Add(matchingField);
            }

            return matches;
        }

        private IReadOnlyCollection<TKind> GetFieldsOfType<TResource, TKind>()
            where TKind : ResourceFieldAttribute
        {
            ResourceType resourceType = GetResourceType(typeof(TResource));

            if (typeof(TKind) == typeof(AttrAttribute))
            {
                return (IReadOnlyCollection<TKind>)resourceType.Attributes;
            }

            if (typeof(TKind) == typeof(RelationshipAttribute))
            {
                return (IReadOnlyCollection<TKind>)resourceType.Relationships;
            }

            return (IReadOnlyCollection<TKind>)resourceType.Fields;
        }

        private IEnumerable<string> ToMemberNames<TResource>(Expression<Func<TResource, dynamic>> selector)
        {
            Expression selectorBody = RemoveConvert(selector.Body);

            if (selectorBody is MemberExpression memberExpression)
            {
                // model => model.Field1

                yield return memberExpression.Member.Name;
            }
            else if (selectorBody is NewExpression newExpression)
            {
                // model => new { model.Field1, model.Field2 }

                foreach (MemberInfo member in newExpression.Members ?? Enumerable.Empty<MemberInfo>())
                {
                    yield return member.Name;
                }
            }
            else
            {
                throw new ArgumentException(
                    $"The expression '{selector}' should select a single property or select multiple properties into an anonymous type. " +
                    "For example: 'article => article.Title' or 'article => new { article.Title, article.PageCount }'.");
            }
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
    }
}
