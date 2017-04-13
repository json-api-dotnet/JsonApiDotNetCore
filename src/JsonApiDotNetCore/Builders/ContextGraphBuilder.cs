using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public class ContextGraphBuilder : IContextGraphBuilder
    {
        private List<ContextEntity> Entities;
        private bool _usesDbContext;
        public ContextGraphBuilder()
        {
            Entities = new List<ContextEntity>();
        }

        public IContextGraph Build()
        {
            var graph = new ContextGraph()
            {
                Entities = Entities,
                UsesDbContext = _usesDbContext
            };

            return graph;
        }

        public void AddResource<TResource>(string pluralizedTypeName) where TResource : class
        {
            var entityType = typeof(TResource);
            Entities.Add(new ContextEntity
            {
                EntityName = pluralizedTypeName,
                EntityType = entityType,
                Attributes = GetAttributes(entityType),
                Relationships = GetRelationships(entityType)
            });
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

            var entities = new List<ContextEntity>();

            var contextProperties = contextType.GetProperties();

            foreach (var property in contextProperties)
            {
                var dbSetType = property.PropertyType;

                if (dbSetType.GetTypeInfo().IsGenericType
                    && dbSetType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = dbSetType.GetGenericArguments()[0];
                    entities.Add(new ContextEntity
                    {
                        EntityName = property.Name,
                        EntityType = entityType,
                        Attributes = GetAttributes(entityType),
                        Relationships = GetRelationships(entityType)
                    });
                }
            }

            Entities = entities;
        }
    }
}
