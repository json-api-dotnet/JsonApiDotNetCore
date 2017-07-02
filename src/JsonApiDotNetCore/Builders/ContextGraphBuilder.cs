using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Extensions;
using System.Linq;

namespace JsonApiDotNetCore.Builders
{
    public class ContextGraphBuilder : IContextGraphBuilder
    {
        private List<ContextEntity> _entities = new List<ContextEntity>();
        private bool _usesDbContext;
        public Link DocumentLinks { get; set; } = Link.All;

        public IContextGraph Build()
        {
            // this must be done at build so that call order doesn't matter
            _entities.ForEach(e => e.Links = GetLinkFlags(e.EntityType));

            var graph = new ContextGraph()
            {
                Entities = _entities,
                UsesDbContext = _usesDbContext
            };
            return graph;
        }

        public void AddResource<TResource>(string pluralizedTypeName) where TResource : class
        {
            var entityType = typeof(TResource);

            VerifyEntityIsNotAlreadyDefined(entityType);

            _entities.Add(new ContextEntity
            {
                EntityName = pluralizedTypeName,
                EntityType = entityType,
                Attributes = GetAttributes(entityType),
                Relationships = GetRelationships(entityType)
            });
        }

        private Link GetLinkFlags(Type entityType)
        {
            var attribute = (LinksAttribute)entityType.GetTypeInfo().GetCustomAttribute(typeof(LinksAttribute));
            if (attribute != null)
                return attribute.Links;

            return DocumentLinks;
        }

        protected virtual List<AttrAttribute> GetAttributes(Type entityType)
        {
            var attributes = new List<AttrAttribute>();

            var properties = entityType.GetProperties();

            foreach (var prop in properties)
            {
                var attribute = (AttrAttribute)prop.GetCustomAttribute(typeof(AttrAttribute));
                if (attribute == null) continue;
                attribute.InternalAttributeName = prop.Name;
                attributes.Add(attribute);
            }
            return attributes;
        }

        protected virtual List<RelationshipAttribute> GetRelationships(Type entityType)
        {
            var attributes = new List<RelationshipAttribute>();

            var properties = entityType.GetProperties();

            foreach (var prop in properties)
            {
                var attribute = (RelationshipAttribute)prop.GetCustomAttribute(typeof(RelationshipAttribute));
                if (attribute == null) continue;
                attribute.InternalRelationshipName = prop.Name;
                attribute.Type = GetRelationshipType(attribute, prop);
                attributes.Add(attribute);
            }
            return attributes;
        }

        protected virtual Type GetRelationshipType(RelationshipAttribute relation, PropertyInfo prop)
        {
            if (relation.IsHasMany)
                return prop.PropertyType.GetGenericArguments()[0];
            else
                return prop.PropertyType;
        }

        public void AddDbContext<T>() where T : DbContext
        {
            _usesDbContext = true;

            var contextType = typeof(T);

            var contextProperties = contextType.GetProperties();

            foreach (var property in contextProperties)
            {
                var dbSetType = property.PropertyType;

                if (dbSetType.GetTypeInfo().IsGenericType
                    && dbSetType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = dbSetType.GetGenericArguments()[0];

                    VerifyEntityIsNotAlreadyDefined(entityType);

                    _entities.Add(new ContextEntity
                    {
                        EntityName = GetResourceName(property),
                        EntityType = entityType,
                        Attributes = GetAttributes(entityType),
                        Relationships = GetRelationships(entityType)
                    });
                }
            }
        }

        private string GetResourceName(PropertyInfo property)
        {
            var resourceAttribute = property.GetCustomAttribute(typeof(ResourceAttribute));
            if (resourceAttribute == null)
                return property.Name.Dasherize();

            return ((ResourceAttribute)resourceAttribute).ResourceName;
        }

        private void VerifyEntityIsNotAlreadyDefined(Type entityType)
        {
            if (_entities.Any(e => e.EntityType == entityType))
                throw new InvalidOperationException($"Cannot add entity type {entityType} to context graph, there is already an entity of that type configured.");
        }
    }
}
