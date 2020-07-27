using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    ///  keeps track of all the models/resources defined in JADNC
    /// </summary>
    public class ResourceGraph : IResourceGraph
    {
        private List<ResourceContext> Resources { get; }
        private Type ProxyInterface { get; }

        public ResourceGraph(List<ResourceContext> resources)
        {
            Resources = resources;
            ProxyInterface = Type.GetType("Castle.DynamicProxy.IProxyTargetAccessor, Castle.Core");
        }

        /// <inheritdoc />
        public IEnumerable<ResourceContext> GetResourceContexts() => Resources;
        /// <inheritdoc />
        public ResourceContext GetResourceContext(string resourceName)
            => Resources.SingleOrDefault(e => e.ResourceName == resourceName);
        /// <inheritdoc />
        public ResourceContext GetResourceContext(Type resourceType)
            => IsDynamicProxy(resourceType) ?
                Resources.SingleOrDefault(e => e.ResourceType == resourceType.BaseType) :
                Resources.SingleOrDefault(e => e.ResourceType == resourceType);
        /// <inheritdoc />
        public ResourceContext GetResourceContext<TResource>() where TResource : class, IIdentifiable
            => GetResourceContext(typeof(TResource));
        /// <inheritdoc/>
        public List<IResourceField> GetFields<T>(Expression<Func<T, dynamic>> selector = null) where T : IIdentifiable
        {
            return Getter(selector).ToList();
        }
        /// <inheritdoc/>
        public List<AttrAttribute> GetAttributes<T>(Expression<Func<T, dynamic>> selector = null) where T : IIdentifiable
        {
            return Getter(selector, FieldFilterType.Attribute).Cast<AttrAttribute>().ToList();
        }
        /// <inheritdoc/>
        public List<RelationshipAttribute> GetRelationships<T>(Expression<Func<T, dynamic>> selector = null) where T : IIdentifiable
        {
            return Getter(selector, FieldFilterType.Relationship).Cast<RelationshipAttribute>().ToList();
        }
        /// <inheritdoc/>
        public List<IResourceField> GetFields(Type type)
        {
            return GetResourceContext(type).Fields.ToList();
        }
        /// <inheritdoc/>
        public List<AttrAttribute> GetAttributes(Type type)
        {
            return GetResourceContext(type).Attributes.ToList();
        }
        /// <inheritdoc/>
        public List<RelationshipAttribute> GetRelationships(Type type)
        {
            return GetResourceContext(type).Relationships.ToList();
        }
        /// <inheritdoc />
        public RelationshipAttribute GetInverse(RelationshipAttribute relationship)
        {
            if (relationship.InverseNavigation == null) return null;
            return GetResourceContext(relationship.RightType)
                            .Relationships
                            .SingleOrDefault(r => r.PropertyInfo.Name == relationship.InverseNavigation);
        }

        private IEnumerable<IResourceField> Getter<T>(Expression<Func<T, dynamic>> selector = null, FieldFilterType type = FieldFilterType.None) where T : IIdentifiable
        {
            IEnumerable<IResourceField> available;
            if (type == FieldFilterType.Attribute)
                available = GetResourceContext(typeof(T)).Attributes;
            else if (type == FieldFilterType.Relationship)
                available = GetResourceContext(typeof(T)).Relationships;
            else
                available = GetResourceContext(typeof(T)).Fields;

            if (selector == null)
                return available;

            var targeted = new List<IResourceField>();

            var selectorBody = RemoveConvert(selector.Body);

            if (selectorBody is MemberExpression memberExpression)
            {   // model => model.Field1
                try
                {
                    targeted.Add(available.Single(f => f.PropertyName == memberExpression.Member.Name));
                    return targeted;
                }
                catch (InvalidOperationException)
                {
                    ThrowNotExposedError(memberExpression.Member.Name, type);
                }
            }

            if (selectorBody is NewExpression newExpression)
            {   // model => new { model.Field1, model.Field2 }
                string memberName = null;
                try
                {
                    if (newExpression.Members == null)
                        return targeted;

                    foreach (var member in newExpression.Members)
                    {
                        memberName = member.Name;
                        targeted.Add(available.Single(f => f.PropertyName == memberName));
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
                $"For example: 'article => article.Title' or 'article => new {{ article.Title, article.PageCount }}'.");
        }

        private bool IsDynamicProxy(Type resourceType) => ProxyInterface?.IsAssignableFrom(resourceType) ?? false;

        private static Expression RemoveConvert(Expression expression)
            => expression is UnaryExpression unaryExpression
               && unaryExpression.NodeType == ExpressionType.Convert
                ? RemoveConvert(unaryExpression.Operand)
                : expression;

        private void ThrowNotExposedError(string memberName, FieldFilterType type)
        {
            throw new ArgumentException($"{memberName} is not an json:api exposed {type:g}.");
        }

        /// <summary>
        /// internally used only by <see cref="ResourceGraph"/>.
        /// </summary>
        private enum FieldFilterType
        {
            None,
            Attribute,
            Relationship
        }
    }
}
