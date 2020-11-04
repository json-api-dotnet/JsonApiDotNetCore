using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories.Internal
{
    internal sealed class ThroughEntitiesFilter
    {
        private readonly DbContext _dbContext;
        private readonly HasManyThroughAttribute _relationship;

        internal ThroughEntitiesFilter(DbContext dbContext, HasManyThroughAttribute relationship)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
        }

        public async Task<object[]> GetBy(object primaryId, ISet<object> secondaryIds)
        {
                                                                                             
            var throughEntityParameter = Expression.Parameter(_relationship.ThroughType, _relationship.ThroughType.Name.Camelize());
            var filter = GetEqualsAndContainsFilter(primaryId, secondaryIds, _relationship, throughEntityParameter);

            dynamic runtimeTypeParameter = TypeHelper.CreateInstance(_relationship.ThroughType);
            dynamic @this = this;

            return await @this.GetFilteredEntities(runtimeTypeParameter, throughEntityParameter, filter);
        }

        private async Task<object[]> GetFilteredEntities<TThroughType>(TThroughType _, ParameterExpression parameter, Expression filter) where TThroughType : class
        {
            var predicate = Expression.Lambda<Func<TThroughType, bool>>(filter, parameter);
            var result = await _dbContext.Set<TThroughType>().Where(predicate).ToListAsync();
            
            return result.Cast<object>().ToArray();
        }

        internal static Expression GetEqualsAndContainsFilter(object idToEqual, ISet<object> idsToContain,
            HasManyThroughAttribute relationship, ParameterExpression parameter)
        {
            var idEqualsFilter = GetEqualsCall(idToEqual, parameter, relationship.LeftIdProperty);
            var containsIdFilter = GetContainsCall(idsToContain, parameter, relationship.RightIdProperty);

            return Expression.AndAlso(idEqualsFilter, containsIdFilter);
        }

        internal static MethodCallExpression GetContainsCall(ISet<object> secondaryResourceIds,
            ParameterExpression rightEntityParameter, PropertyInfo rightIdProperty)
        {
            var rightIdMember = Expression.Property(rightEntityParameter, rightIdProperty.Name);

            var idType = rightIdProperty.PropertyType;
            var typedIds = TypeHelper.CopyToList(secondaryResourceIds, idType);
            var idCollectionConstant = Expression.Constant(typedIds);

            var containsCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                new[] {idType},
                idCollectionConstant,
                rightIdMember);

            return containsCall;
        }

        internal static BinaryExpression GetEqualsCall(object id, ParameterExpression rightEntityParameter,
            PropertyInfo leftIdProperty)
        {
            var leftIdMember = Expression.Property(rightEntityParameter, leftIdProperty.Name);
            var idConstant = Expression.Constant(id, id.GetType());

            return Expression.Equal(leftIdMember, idConstant);
        }
    }
}
