using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public class RepositoryRelationshipUpdateHelper<TRelatedResource> : IRepositoryRelationshipUpdateHelper where TRelatedResource : class
    {
        private readonly IResourceFactory _resourceFactory;
        private readonly DbContext _context;

        public RepositoryRelationshipUpdateHelper(IDbContextResolver contextResolver, IResourceFactory resourceFactory)
        {
            if (contextResolver == null) throw new ArgumentNullException(nameof(contextResolver));

            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _context = contextResolver.GetContext();
        }

        /// <inheritdoc />
        public virtual async Task UpdateRelationshipAsync(IIdentifiable parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (relationshipIds == null) throw new ArgumentNullException(nameof(relationshipIds));

            if (relationship is HasManyThroughAttribute hasManyThrough)
                await UpdateManyToManyAsync(parent, hasManyThrough, relationshipIds);
            else if (relationship is HasManyAttribute)
                await UpdateOneToManyAsync(parent, relationship, relationshipIds);
            else
                await UpdateOneToOneAsync(parent, relationship, relationshipIds);
        }

        private async Task UpdateOneToOneAsync(IIdentifiable parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds)
        {
            TRelatedResource value = null;
            if (relationshipIds.Any())
            {   // newOwner.id
                var target = Expression.Constant(TypeHelper.ConvertType(relationshipIds.First(), TypeHelper.GetIdType(relationship.RightType)));
                // (Person p) => ...
                ParameterExpression parameter = Expression.Parameter(typeof(TRelatedResource));
                // (Person p) => p.Id
                Expression idMember = Expression.Property(parameter, nameof(Identifiable.Id));
                // newOwner.Id.Equals(p.Id)
                Expression callEquals = Expression.Call(idMember, nameof(object.Equals), null, target);
                var equalsLambda = Expression.Lambda<Func<TRelatedResource, bool>>(callEquals, parameter);
                value = await _context.Set<TRelatedResource>().FirstOrDefaultAsync(equalsLambda);
            }
            relationship.SetValue(parent, value, _resourceFactory);
        }

        private async Task UpdateOneToManyAsync(IIdentifiable parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds)
        {
            IEnumerable value;
            if (!relationshipIds.Any())
            {
                var collectionType = TypeHelper.ToConcreteCollectionType(relationship.Property.PropertyType);
                value = (IEnumerable)TypeHelper.CreateInstance(collectionType);
            }
            else
            {
                var idType = TypeHelper.GetIdType(relationship.RightType);
                var typedIds = TypeHelper.CopyToList(relationshipIds, idType, stringId => TypeHelper.ConvertType(stringId, idType));

                // [1, 2, 3]
                var target = Expression.Constant(typedIds);
                // (Person p) => ...
                ParameterExpression parameter = Expression.Parameter(typeof(TRelatedResource));
                // (Person p) => p.Id
                Expression idMember = Expression.Property(parameter, nameof(Identifiable.Id));
                // [1,2,3].Contains(p.Id)
                var callContains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { idMember.Type }, target, idMember);
                var containsLambda = Expression.Lambda<Func<TRelatedResource, bool>>(callContains, parameter);

                var resultSet = await _context.Set<TRelatedResource>().Where(containsLambda).ToListAsync();
                value = TypeHelper.CopyToTypedCollection(resultSet, relationship.Property.PropertyType);
            }

            relationship.SetValue(parent, value, _resourceFactory);
        }

        private async Task UpdateManyToManyAsync(IIdentifiable parent, HasManyThroughAttribute relationship, IReadOnlyCollection<string> relationshipIds)
        {
            // we need to create a transaction for the HasManyThrough case so we can get and remove any existing
            // through resources and only commit if all operations are successful
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
                // TODO: [#696] Potential location where we crash if the relationship targets an abstract base class.
                var link = _resourceFactory.CreateInstance(relationship.ThroughType);
                relationship.LeftIdProperty.SetValue(link, TypeHelper.ConvertType(parentId, relationship.LeftIdProperty.PropertyType));
                relationship.RightIdProperty.SetValue(link, TypeHelper.ConvertType(x, relationship.RightIdProperty.PropertyType));
                return link;
            });

            _context.AddRange(newLinks);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
