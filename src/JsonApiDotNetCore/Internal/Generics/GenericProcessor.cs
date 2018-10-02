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

    public class GenericProcessor<T> : IGenericProcessor where T : class
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
            if (relationship is HasManyThroughAttribute hasManyThrough && parent is IIdentifiable identifiableParent)
            {
                // ArticleTag
                ParameterExpression parameter = Expression.Parameter(hasManyThrough.ThroughType);

                // ArticleTag.ArticleId
                Expression property = Expression.Property(parameter, hasManyThrough.LeftIdProperty);

                // article.Id
                var parentId = TypeHelper.ConvertType(identifiableParent.StringId, hasManyThrough.LeftIdProperty.PropertyType);
                Expression target = Expression.Constant(parentId);

                // ArticleTag.ArticleId.Equals(article.Id)
                Expression equals = Expression.Call(property, "Equals", null, target);

                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

                var oldLinks = _context
                    .Set<T>()
                    .Where(lambda.Compile())
                    .ToList();

                // TODO: we shouldn't need to do this and it especially shouldn't happen outside a transaction
                //       instead we should try updating the existing?
                _context.RemoveRange(oldLinks);

                var newLinks = relationshipIds.Select(x => {
                    var link = Activator.CreateInstance(hasManyThrough.ThroughType);
                    hasManyThrough.LeftIdProperty.SetValue(link, TypeHelper.ConvertType(parentId, hasManyThrough.LeftIdProperty.PropertyType));
                    hasManyThrough.RightIdProperty.SetValue(link, TypeHelper.ConvertType(x, hasManyThrough.RightIdProperty.PropertyType));
                    return link;
                });

                _context.AddRange(newLinks);
            }
            else if (relationship.IsHasMany)
            {
                // TODO: need to handle the failure mode when the relationship does not implement IIdentifiable
                var entities = _context.Set<T>().Where(x => relationshipIds.Contains(((IIdentifiable)x).StringId)).ToList();
                relationship.SetValue(parent, entities);
            }
            else
            {
                // TODO: need to handle the failure mode when the relationship does not implement IIdentifiable
                var entity = _context.Set<T>().SingleOrDefault(x => relationshipIds.First() == ((IIdentifiable)x).StringId);
                relationship.SetValue(parent, entity);
            }
        }
    }
}
