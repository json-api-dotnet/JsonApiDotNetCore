using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraph<T> : IContextGraph where T : DbContext
    {
        public List<ContextEntity> Entities { get; set; }

        public object GetRelationship<TParent>(TParent entity, string relationshipName)
        {
            var parentEntityType = typeof(TParent);

            var navigationProperty = parentEntityType
                .GetProperties()
                .FirstOrDefault(p => p.Name.ToLower() == relationshipName.ToLower());

            if(navigationProperty == null)
                return null;

            return navigationProperty.GetValue(entity);
        }

        public string GetRelationshipName<TParent>(string relationshipName)
        {
            var entityType = typeof(TParent);
            return Entities
                .FirstOrDefault(e => 
                    e.EntityType == entityType)
                .Relationships
                .FirstOrDefault(r => 
                    r.RelationshipName.ToLower() == relationshipName.ToLower())
                ?.RelationshipName;
        }
    }
}
