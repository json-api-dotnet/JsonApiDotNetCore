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
    /// <summary>
    /// A special helper service that gets instantiated for the right-type of a many-to-many relationship and is responsible for
    /// processing updates for that relationships
    /// </summary>
    public interface IHasManyThroughUpdateHelper
    {
        Task UpdateAsync(IIdentifiable parent, HasManyThroughAttribute relationship, IEnumerable<string> relationshipIds);
    }

    /// <summary>
    /// A special processor that gets instantiated for a generic type (&lt;T&gt;)
    /// when the actual type is not known until runtime. Specifically, this is used for updating
    /// relationships.
    /// </summary>
    public class HasManyThroughUpdateHelper<T> : IHasManyThroughUpdateHelper where T : class
    {
        private readonly DbContext _context;
        public HasManyThroughUpdateHelper(IDbContextResolver contextResolver)
        {
            _context = contextResolver.GetContext();
        }

        public virtual async Task UpdateAsync(IIdentifiable parent, HasManyThroughAttribute relationship, IEnumerable<string> relationshipIds)
        {
            // we need to create a transaction for the HasManyThrough case so we can get and remove any existing
            // join entities and only commit if all operations are successful
            using (var transaction = await _context.GetCurrentOrCreateTransactionAsync())
            {
                // ArticleTag
                ParameterExpression parameter = Expression.Parameter(relationship.ThroughType);

                // ArticleTag.ArticleId
                Expression property = Expression.Property(parameter, relationship.LeftIdProperty);

                // article.Id
                var parentId = TypeHelper.ConvertType(parent.StringId, relationship.LeftIdProperty.PropertyType);
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
                    var link = Activator.CreateInstance(relationship.ThroughType);
                    relationship.LeftIdProperty.SetValue(link, TypeHelper.ConvertType(parentId, relationship.LeftIdProperty.PropertyType));
                    relationship.RightIdProperty.SetValue(link, TypeHelper.ConvertType(x, relationship.RightIdProperty.PropertyType));
                    return link;
                });

                _context.AddRange(newLinks);
                await _context.SaveChangesAsync();

                transaction.Commit();
            }
        }
    }
}
