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
    public class ResourceGraph : IResourceGraphExplorer
    {
        internal List<ValidationResult> ValidationResults { get; }
        private List<ContextEntity> _entities { get; }

        public ResourceGraph(List<ContextEntity> entities, List<ValidationResult> validationResults = null)
        {
            _entities = entities;
            ValidationResults = validationResults;
        }

        /// <inheritdoc />
        public ContextEntity[] GetContextEntities() => _entities.ToArray();

        /// <inheritdoc />
        public ContextEntity GetContextEntity(string entityName)
            => _entities.SingleOrDefault(e => string.Equals(e.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc />
        public ContextEntity GetContextEntity(Type entityType)
            => _entities.SingleOrDefault(e => e.EntityType == entityType);
        /// <inheritdoc />
        public ContextEntity GetContextEntity<TResource>() where TResource : class, IIdentifiable
            => GetContextEntity(typeof(TResource));

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
            return GetContextEntity(type).Fields.ToList();
        }
        /// <inheritdoc/>
        public List<AttrAttribute> GetAttributes(Type type)
        {
            return GetContextEntity(type).Attributes.ToList();
        }
        /// <inheritdoc/>
        public List<RelationshipAttribute> GetRelationships(Type type)
        {
            return GetContextEntity(type).Relationships.ToList();
        }

        /// <inheritdoc />
        public RelationshipAttribute GetInverse(RelationshipAttribute relationship)
        {
            if (relationship.InverseNavigation == null) return null;
            return GetContextEntity(relationship.DependentType)
                            .Relationships
                            .SingleOrDefault(r => r.InternalRelationshipName == relationship.InverseNavigation);
        }

        private IEnumerable<IResourceField> Getter<T>(Expression<Func<T, dynamic>> selector = null, FieldFilterType type = FieldFilterType.None) where T : IIdentifiable
        {
            IEnumerable<IResourceField> available;
            if (type == FieldFilterType.Attribute)
                available = GetContextEntity(typeof(T)).Attributes.Cast<IResourceField>();
            else if (type == FieldFilterType.Relationship)
                available = GetContextEntity(typeof(T)).Relationships.Cast<IResourceField>();
            else
                available = GetContextEntity(typeof(T)).Fields;

            if (selector == null)
                return available;

            var targeted = new List<IResourceField>();

            if (selector.Body is MemberExpression memberExpression)
            {   // model => model.Field1
                try
                {
                    targeted.Add(available.Single(f => f.ExposedInternalMemberName == memberExpression.Member.Name));
                    return targeted;
                }
                catch (Exception ex)
                {
                    ThrowNotExposedError(memberExpression.Member.Name, type);
                }
            }


            if (selector.Body is NewExpression newExpression)
            {   // model => new { model.Field1, model.Field2 }
                string memberName = null;
                try
                {
                    if (newExpression.Members == null)
                        return targeted;

                    foreach (var member in newExpression.Members)
                    {
                        memberName = member.Name;
                        targeted.Add(available.Single(f => f.ExposedInternalMemberName == memberName));
                    }
                    return targeted;
                }
                catch (Exception ex)
                {
                    ThrowNotExposedError(memberName, type);
                }
            }

            throw new ArgumentException($"The expression returned by '{selector}' for '{GetType()}' is of type {selector.Body.GetType()}"
                        + " and cannot be used to select resource attributes. The type must be a NewExpression.Example: article => new { article.Author };");

        }

        private void ThrowNotExposedError(string memberName, FieldFilterType type)
        {
            throw new ArgumentException($"{memberName} is not an json:api exposed {type.ToString("g")}.");
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
