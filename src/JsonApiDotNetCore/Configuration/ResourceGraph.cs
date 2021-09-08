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

        private readonly IReadOnlySet<ResourceContext> _resourceContextSet;
        private readonly Dictionary<Type, ResourceContext> _resourceContextsByType = new();
        private readonly Dictionary<string, ResourceContext> _resourceContextsByPublicName = new();

        public ResourceGraph(IReadOnlySet<ResourceContext> resourceContexts)
        {
            ArgumentGuard.NotNull(resourceContexts, nameof(resourceContexts));

            _resourceContextSet = resourceContexts;

            foreach (ResourceContext resourceContext in resourceContexts)
            {
                _resourceContextsByType.Add(resourceContext.ResourceType, resourceContext);
                _resourceContextsByPublicName.Add(resourceContext.PublicName, resourceContext);
            }
        }

        /// <inheritdoc />
        public IReadOnlySet<ResourceContext> GetResourceContexts()
        {
            return _resourceContextSet;
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext(string publicName)
        {
            ResourceContext resourceContext = TryGetResourceContext(publicName);

            if (resourceContext == null)
            {
                throw new InvalidOperationException($"Resource type '{publicName}' does not exist.");
            }

            return resourceContext;
        }

        /// <inheritdoc />
        public ResourceContext TryGetResourceContext(string publicName)
        {
            ArgumentGuard.NotNullNorEmpty(publicName, nameof(publicName));

            return _resourceContextsByPublicName.TryGetValue(publicName, out ResourceContext resourceContext) ? resourceContext : null;
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext(Type resourceType)
        {
            ResourceContext resourceContext = TryGetResourceContext(resourceType);

            if (resourceContext == null)
            {
                throw new InvalidOperationException($"Resource of type '{resourceType.Name}' does not exist.");
            }

            return resourceContext;
        }

        /// <inheritdoc />
        public ResourceContext TryGetResourceContext(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            Type typeToFind = IsLazyLoadingProxyForResourceType(resourceType) ? resourceType.BaseType : resourceType;
            return _resourceContextsByType.TryGetValue(typeToFind!, out ResourceContext resourceContext) ? resourceContext : null;
        }

        private bool IsLazyLoadingProxyForResourceType(Type resourceType)
        {
            return ProxyTargetAccessorType?.IsAssignableFrom(resourceType) ?? false;
        }

        /// <inheritdoc />
        public ResourceContext GetResourceContext<TResource>()
            where TResource : class, IIdentifiable
        {
            return GetResourceContext(typeof(TResource));
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
            ResourceContext resourceContext = GetResourceContext(typeof(TResource));

            if (typeof(TKind) == typeof(AttrAttribute))
            {
                return (IReadOnlyCollection<TKind>)resourceContext.Attributes;
            }

            if (typeof(TKind) == typeof(RelationshipAttribute))
            {
                return (IReadOnlyCollection<TKind>)resourceContext.Relationships;
            }

            return (IReadOnlyCollection<TKind>)resourceContext.Fields;
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
