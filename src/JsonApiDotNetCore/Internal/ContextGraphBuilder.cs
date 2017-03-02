using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraphBuilder<T> where T : DbContext
    {
        private readonly Type _contextType = typeof(T);
        private List<ContextEntity> _entities;

        public ContextGraph<T> Build()
        {
            _getFirstLevelEntities();

            var graph = new ContextGraph<T>
            {
                Entities = _entities
            };

            return graph;            
        }

        private void _getFirstLevelEntities()
        {
            var entities = new List<ContextEntity>();

            var contextProperties = _contextType.GetProperties();
            
            foreach(var property in contextProperties)
            {
                var dbSetType = property.PropertyType;
                
                if (dbSetType.GetTypeInfo().IsGenericType 
                    && dbSetType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = dbSetType.GetGenericArguments()[0];
                    entities.Add(new ContextEntity {
                        EntityName = property.Name,
                        EntityType = entityType,
                        Attributes = _getAttributes(entityType),
                        Relationships = _getRelationships(entityType)
                    });
                }                    
            }

            _entities = entities;
        }

        private List<AttrAttribute> _getAttributes(Type entityType)
        {
            var attributes = new List<AttrAttribute>();
            
            var properties = entityType.GetProperties();

            foreach(var prop in properties)
            {
                var attribute = (AttrAttribute)prop.GetCustomAttribute(typeof(AttrAttribute));
                if(attribute == null) continue;
                attribute.InternalAttributeName = prop.Name;
                attributes.Add(attribute);
            }
            return attributes;
        }

        private List<RelationshipAttribute> _getRelationships(Type entityType)
        {
            var attributes = new List<RelationshipAttribute>();
            
            var properties = entityType.GetProperties();

            foreach(var prop in properties)
            {
                var attribute = (RelationshipAttribute)prop.GetCustomAttribute(typeof(RelationshipAttribute));
                if(attribute == null) continue;
                attribute.InternalRelationshipName = prop.Name;
                attribute.Type = _getRelationshipType(attribute, prop);
                attributes.Add(attribute);
            }
            return attributes;
        }

        private Type _getRelationshipType(RelationshipAttribute relation, PropertyInfo prop)
        {
            if(relation.IsHasMany)
                return prop.PropertyType.GetGenericArguments()[0];
            else
                return prop.PropertyType;
        }
    }
}
