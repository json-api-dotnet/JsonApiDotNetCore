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
    /// A special helper that processes updates of relationships
    /// </summary>
    /// <remarks>
    /// This service required to be able translate involved expressions into queries
    /// instead of having them evaluated on the client side. In particular, for all three types of relationship
    /// a lookup is performed based on an id. Expressions that use IIdentifiable.StringId can never
    /// be translated into queries because this property only exists at runtime after the query is performed.
    /// We will have to build expression trees if we want to use IIdentifiable{TId}.TId, for which we minimally a
    /// generic execution to DbContext.Set{T}().
    /// </remarks>
    public interface IRepositoryRelationshipUpdateHelper
    {
        /// <summary>
        /// Processes updates of relationships
        /// </summary>
        Task UpdateRelationshipAsync(IIdentifiable parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
    }

    /// <inheritdoc/>
    public class RepositoryRelationshipUpdateHelper<TRelatedResource> : IRepositoryRelationshipUpdateHelper where TRelatedResource : class
    {
        private readonly DbContext _context;
        public RepositoryRelationshipUpdateHelper(IDbContextResolver contextResolver)
        {
            _context = contextResolver.GetContext();
        }

        /// <inheritdoc/>
        public virtual async Task UpdateRelationshipAsync(IIdentifiable parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            if (relationship is HasManyThroughAttribute hasManyThrough)
                await UpdateManyToManyAsync(parent, hasManyThrough, relationshipIds);
            else if (relationship is HasManyAttribute)
                await UpdateOneToManyAsync(parent, relationship, relationshipIds);
            else
                await UpdateOneToOneAsync(parent, relationship, relationshipIds);

        }

        private async Task UpdateOneToOneAsync(IIdentifiable parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            TRelatedResource value = null;
            if (relationshipIds.Any())
            {   // newOwner.id
                var target = Expression.Constant(TypeHelper.ConvertType(relationshipIds.First(), TypeHelper.GetIdentifierType(relationship.RightType)));
                // (Person p) => ...
                ParameterExpression parameter = Expression.Parameter(typeof(TRelatedResource));
                // (Person p) => p.Id
                Expression idMember = Expression.Property(parameter, nameof(Identifiable.Id));
                // newOwner.Id.Equals(p.Id)
                Expression callEquals = Expression.Call(idMember, nameof(object.Equals), null, target);
                var equalsLambda = Expression.Lambda<Func<TRelatedResource, bool>>(callEquals, parameter);
                value = await _context.Set<TRelatedResource>().FirstOrDefaultAsync(equalsLambda);
            }
            relationship.SetValue(parent, value);
        }

        private async Task UpdateOneToManyAsync(IIdentifiable parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            var value = new List<TRelatedResource>();
            if (relationshipIds.Any())
            {   // [1, 2, 3]
                var target = Expression.Constant(TypeHelper.ConvertListType(relationshipIds, TypeHelper.GetIdentifierType(relationship.RightType)));
                // (Person p) => ...
                ParameterExpression parameter = Expression.Parameter(typeof(TRelatedResource));
                // (Person p) => p.Id
                Expression idMember = Expression.Property(parameter, nameof(Identifiable.Id));
                // [1,2,3].Contains(p.Id)
                var callContains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { idMember.Type }, target, idMember);
                var containsLamdda = Expression.Lambda<Func<TRelatedResource, bool>>(callContains, parameter);
                value = await _context.Set<TRelatedResource>().Where(containsLamdda).ToListAsync();
            }
            relationship.SetValue(parent, value);
        }

        private async Task UpdateManyToManyAsync(IIdentifiable parent, HasManyThroughAttribute relationship, IEnumerable<string> relationshipIds)
        {
            // we need to create a transaction for the HasManyThrough case so we can get and remove any existing
            // join entities and only commit if all operations are successful
            var transaction = await _context.GetCurrentOrCreateTransactionAsync();
            // ArticleTag
            ParameterExpression parameter = Expression.Parameter(relationship.ThroughType);
            // ArticleTag.ArticleId
            Expression idMember = Expression.Property(parameter, relationship.LeftIdProperty);
            // article.Id
            var parentId = TypeHelper.ConvertType(parent.StringId, relationship.LeftIdProperty.PropertyType);
            Expression target = Expression.Constant(parentId);
            // ArticleTag.ArticleId.Equals(article.Id)
            Expression callEquals = Expression.Call(idMember, "Equals", null, target);
            var lambda = Expression.Lambda<Func<TRelatedResource, bool>>(callEquals, parameter);
            // TODO: we shouldn't need to do this instead we should try updating the existing?
            // the challenge here is if a composite key is used, then we will fail to 
            // create due to a unique key violation
            var oldLinks = _context
                .Set<TRelatedResource>()
                .Where(lambda.Compile())
                .ToList();

            _context.RemoveRange(oldLinks);

            var newLinks = relationshipIds.Select(x =>
            {
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
