using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal.Generics
{
    public interface IGenericProcessor
    {
        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
        void SetRelationships(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
    }

    public class GenericProcessor<T> : IGenericProcessor where T : class, IIdentifiable
    {
        private readonly DbContext _context;
        public GenericProcessor(IDbContextResolver contextResolver)
        {
            _context = contextResolver.GetContext();
        }

        public virtual async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            SetRelationships(parent, relationship, relationshipIds);

            await _context.SaveChangesAsync();
        }

        public virtual void SetRelationships(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            if (relationship is HasManyThroughAttribute hasManyThrough)
            {
                var parentId = ((IIdentifiable)parent).StringId;
                ParameterExpression parameter = Expression.Parameter(hasManyThrough.Type);
                Expression property = Expression.Property(parameter, hasManyThrough.LeftProperty);
                Expression target = Expression.Constant(parentId);
                Expression toString = Expression.Call(property, "ToString", null, null);
                Expression equals = Expression.Call(toString, "Equals", null, target);
                Expression<Func<object, bool>> lambda = Expression.Lambda<Func<object, bool>>(equals, parameter);

                var oldLinks = _context
                    .Set(hasManyThrough.ThroughType)
                    .Where(lambda.Compile())
                    .ToList();

                _context.Remove(oldLinks);

                var newLinks = relationshipIds.Select(x => {
                    var link = Activator.CreateInstance(hasManyThrough.ThroughType);
                    hasManyThrough.LeftProperty.SetValue(link, TypeHelper.ConvertType(parent, hasManyThrough.LeftProperty.PropertyType));
                    hasManyThrough.RightProperty.SetValue(link, TypeHelper.ConvertType(x, hasManyThrough.RightProperty.PropertyType));
                    return link;
                });
                _context.AddRange(newLinks);
            }
            else if (relationship.IsHasMany)
            {
                var entities = _context.Set<T>().Where(x => relationshipIds.Contains(x.StringId)).ToList();
                relationship.SetValue(parent, entities);
            }
            else
            {
                var entity = _context.Set<T>().SingleOrDefault(x => relationshipIds.First() == x.StringId);
                relationship.SetValue(parent, entity);
            }
        }
    }
}
