using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraphBuilder<T> where T : DbContext
    {
        private readonly Type _contextType = typeof(T);
        private List<ContextEntity> _entities;

        public ContextGraph<T> Build()
        {
            _getFirstLevelEntities();
            _loadRelationships();

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
                var type = property.PropertyType;
                
                if (type.GetTypeInfo().IsGenericType 
                    && type.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    entities.Add(new ContextEntity {
                        EntityName = property.Name,
                        EntityType = type.GetGenericArguments()[0]
                    });
                }                    
            }

            _entities = entities;
        }

        private void _loadRelationships()
        {          
            _entities.ForEach(entity => {

                var relationships = new List<Relationship>();
                var properties = entity.EntityType.GetProperties();
                
                foreach(var entityProperty in properties)
                {
                    var propertyType = entityProperty.PropertyType;
                    
                    if(_isValidEntity(propertyType) 
                        || (propertyType.GetTypeInfo().IsGenericType && _isValidEntity(propertyType.GetGenericArguments()[0])))
                        relationships.Add(_getRelationshipFromPropertyInfo(entityProperty));
                }

                entity.Relationships = relationships;
            });
        }

        private bool _isValidEntity(Type type)
        {
            var validEntityRelationshipTypes = _entities.Select(e => e.EntityType);
            return validEntityRelationshipTypes.Contains(type);
        }

        private Relationship _getRelationshipFromPropertyInfo(PropertyInfo propertyInfo)
        {
            return new Relationship {
                Type = propertyInfo.PropertyType,
                RelationshipName = propertyInfo.Name
            };
        }
    }
}
