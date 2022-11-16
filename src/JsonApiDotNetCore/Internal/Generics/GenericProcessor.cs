using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.Internal.Generics
{
    // TODO: consider renaming to PatchRelationshipService (or something)
    public interface IGenericProcessor
    {
        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
    }

    /// <summary>
    /// A special processor that gets instantiated for a generic type (&lt;T&gt;)
    /// when the actual type is not known until runtime. Specifically, this is used for updating
    /// relationships.
    /// </summary>
    public class GenericProcessor<T> : IGenericProcessor where T : class
    {
        private readonly DbContext _context;
        public GenericProcessor(IDbContextResolver contextResolver)
        {
            _context = contextResolver.GetContext();
        }

        public virtual async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            if (relationship is HasManyThroughAttribute hasManyThrough && parent is IIdentifiable identifiableParent)
            {
                await SetHasManyThroughRelationshipAsync(identifiableParent, hasManyThrough, relationshipIds);
            }
            else
            {
                await SetRelationshipsAsync(parent, relationship, relationshipIds);
            }
        }

        private async Task SetHasManyThroughRelationshipAsync(IIdentifiable identifiableParent, HasManyThroughAttribute hasManyThrough, IEnumerable<string> relationshipIds)
        {
            // we need to create a transaction for the HasManyThrough case so we can get and remove any existing
            // join entities and only commit if all operations are successful
            using(var transaction = await _context.GetCurrentOrCreateTransactionAsync())
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

                // TODO: we shouldn't need to do this instead we should try updating the existing?
                // the challenge here is if a composite key is used, then we will fail to 
                // create due to a unique key violation
                var oldLinks = _context
                    .Set<T>()
                    .Where(lambda.Compile())
                    .ToList();

                _context.RemoveRange(oldLinks);

                var newLinks = relationshipIds.Select(x => {
                    var link = Activator.CreateInstance(hasManyThrough.ThroughType);
                    hasManyThrough.LeftIdProperty.SetValue(link, TypeHelper.ConvertType(parentId, hasManyThrough.LeftIdProperty.PropertyType));
                    hasManyThrough.RightIdProperty.SetValue(link, TypeHelper.ConvertType(x, hasManyThrough.RightIdProperty.PropertyType));
                    return link;
                });

                _context.AddRange(newLinks);
                await _context.SaveChangesAsync();

                transaction.Commit();
            }
        }

        private async Task SetRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            if (relationship.IsHasMany)
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

            await _context.SaveChangesAsync();
        }
    }
}
