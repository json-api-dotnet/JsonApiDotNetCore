using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Data
{
    /// <inheritdoc />
    public class DefaultEntityRepository<TEntity>
        : DefaultEntityRepository<TEntity, int>,
        IEntityRepository<TEntity>
        where TEntity : class, IIdentifiable<int>
    {
        public DefaultEntityRepository(
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver
            )
        : base(jsonApiContext, contextResolver)
        { }

        public DefaultEntityRepository(
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver

            )
        : base(loggerFactory, jsonApiContext, contextResolver)
        { }
    }

    /// <summary>
    /// Provides a default repository implementation and is responsible for
    /// abstracting any EF Core APIs away from the service layer.
    /// </summary>
    public class DefaultEntityRepository<TEntity, TId>
        : IEntityRepository<TEntity, TId>,
        IEntityFrameworkRepository<TEntity>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly ILogger _logger;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IGenericProcessorFactory _genericProcessorFactory;
        private readonly ResourceDefinition<TEntity> _resourceDefinition;

        public DefaultEntityRepository(
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver
            )
        {
            _context = contextResolver.GetContext();
            _dbSet = contextResolver.GetDbSet<TEntity>();
            _jsonApiContext = jsonApiContext;
            _genericProcessorFactory = _jsonApiContext.GenericProcessorFactory;


        }

        public DefaultEntityRepository(
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver
            )
        {
            _context = contextResolver.GetContext();
            _dbSet = contextResolver.GetDbSet<TEntity>();
            _jsonApiContext = jsonApiContext;
            _logger = loggerFactory.CreateLogger<DefaultEntityRepository<TEntity, TId>>();
            _genericProcessorFactory = _jsonApiContext.GenericProcessorFactory;
            _resourceDefinition = _genericProcessorFactory.GetProcessor<ResourceDefinition<TEntity>>(typeof(ResourceDefinition<>), typeof(TEntity));

        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Get()
        {
            var entities = (IQueryable<TEntity>)_dbSet;
            if (_resourceDefinition != null)
            {
                entities = _resourceDefinition.OnList(entities);
            }

            if (_jsonApiContext.QuerySet?.Fields != null && _jsonApiContext.QuerySet.Fields.Count > 0)
                return entities.Select(_jsonApiContext.QuerySet?.Fields);

            return entities;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery)
        {
            if (_resourceDefinition != null)
            {
                var defaultQueryFilters = _resourceDefinition.GetQueryFilters();
                if (defaultQueryFilters != null && defaultQueryFilters.TryGetValue(filterQuery.Attribute, out var defaultQueryFilter) == true)
                {
                    return defaultQueryFilter(entities, filterQuery.Value);
                }
            }

            return entities.Filter(_jsonApiContext, filterQuery);
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries)
        {
            if (sortQueries != null && sortQueries.Count > 0)
                return entities.Sort(_jsonApiContext, sortQueries);

            if (_resourceDefinition != null)
            {
                var defaultSortOrder = _resourceDefinition.DefaultSort();
                if (defaultSortOrder != null && defaultSortOrder.Count > 0)
                {
                    foreach (var sortProp in defaultSortOrder)
                    {
                        // this is dumb...add an overload, don't allocate for no reason
                        entities.Sort(_jsonApiContext, new SortQuery(sortProp.Item2, sortProp.Item1.PublicAttributeName));
                    }
                }
            }

            return entities;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TId id)
        {
            return await Get().SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            _logger?.LogDebug($"[JADN] GetAndIncludeAsync({id}, {relationshipName})");

            var includedSet = Include(Get(), relationshipName);
            var result = await includedSet.SingleOrDefaultAsync(e => e.Id.Equals(id));

            return result;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            AttachRelationships(entity);
            _dbSet.Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        protected virtual void AttachRelationships(TEntity entity = null)
        {
            AttachHasManyPointers(entity);
            AttachHasOnePointers(entity);
        }

        /// <inheritdoc />
        public void DetachRelationshipPointers(TEntity entity)
        {
            foreach (var hasOneRelationship in _jsonApiContext.HasOneRelationshipPointers.Get())
            {
                var hasOne = (HasOneAttribute)hasOneRelationship.Key;
                if (hasOne.EntityPropertyName != null)
                {
                    var relatedEntity = entity.GetType().GetProperty(hasOne.EntityPropertyName)?.GetValue(entity);
                    if (relatedEntity != null)
                        _context.Entry(relatedEntity).State = EntityState.Detached;
                }
                else
                {
                    _context.Entry(hasOneRelationship.Value).State = EntityState.Detached;
                }
            }

            foreach (var hasManyRelationship in _jsonApiContext.HasManyRelationshipPointers.Get())
            {
                var hasMany = (HasManyAttribute)hasManyRelationship.Key;
                if (hasMany.EntityPropertyName != null)
                {
                    var relatedList = (IList)entity.GetType().GetProperty(hasMany.EntityPropertyName)?.GetValue(entity);
                    foreach (var related in relatedList)
                    {
                        _context.Entry(related).State = EntityState.Detached;
                    }
                }
                else
                {
                    foreach (var pointer in hasManyRelationship.Value)
                    {
                        _context.Entry(pointer).State = EntityState.Detached;
                    }
                }

                // HACK: detaching has many relationships doesn't appear to be sufficient
                // the navigation property actually needs to be nulled out, otherwise
                // EF adds duplicate instances to the collection
                hasManyRelationship.Key.SetValue(entity, null);
            }
        }

        /// <summary>
        /// This is used to allow creation of HasMany relationships when the
        /// dependent side of the relationship already exists.
        /// </summary>
        private void AttachHasManyPointers(TEntity entity)
        {
            var relationships = _jsonApiContext.HasManyRelationshipPointers.Get();
            foreach (var relationship in relationships)
            {
                if (relationship.Key is HasManyThroughAttribute hasManyThrough)
                    AttachHasManyThrough(entity, hasManyThrough, relationship.Value);
                else
                    AttachHasMany(entity, relationship.Key as HasManyAttribute, relationship.Value);
            }
        }

        private void AttachHasMany(TEntity entity, HasManyAttribute relationship, IList pointers)
        {
            if (relationship.EntityPropertyName != null)
            {
                var relatedList = (IList)entity.GetType().GetProperty(relationship.EntityPropertyName)?.GetValue(entity);
                foreach (var related in relatedList)
                {
                    _context.Entry(related).State = EntityState.Unchanged;
                }
            }
            else
            {
                foreach (var pointer in pointers)
                {
                    _context.Entry(pointer).State = EntityState.Unchanged;
                }
            }
        }

        private void AttachHasManyThrough(TEntity entity, HasManyThroughAttribute hasManyThrough, IList pointers)
        {
            // create the collection (e.g. List<ArticleTag>)
            // this type MUST implement IList so we can build the collection
            // if this is problematic, we _could_ reflect on the type and find an Add method
            // or we might be able to create a proxy type and implement the enumerator
            var throughRelationshipCollection = Activator.CreateInstance(hasManyThrough.ThroughProperty.PropertyType) as IList;
            hasManyThrough.ThroughProperty.SetValue(entity, throughRelationshipCollection);

            foreach (var pointer in pointers)
            {
                _context.Entry(pointer).State = EntityState.Unchanged;
                var throughInstance = Activator.CreateInstance(hasManyThrough.ThroughType);

                hasManyThrough.LeftProperty.SetValue(throughInstance, entity);
                hasManyThrough.RightProperty.SetValue(throughInstance, pointer);

                throughRelationshipCollection.Add(throughInstance);
            }
        }

        /// <summary>
        /// This is used to allow creation of HasOne relationships when the
        /// independent side of the relationship already exists.
        /// </summary>
        private void AttachHasOnePointers(TEntity entity)
        {
            var relationships = _jsonApiContext.HasOneRelationshipPointers.Get();
            foreach (var relationship in relationships)
            {
                if (relationship.Key.GetType() != typeof(HasOneAttribute))
                    continue;

                var hasOne = (HasOneAttribute)relationship.Key;
                if (hasOne.EntityPropertyName != null)
                {
                    var relatedEntity = entity.GetType().GetProperty(hasOne.EntityPropertyName)?.GetValue(entity);
                    if (relatedEntity != null && _context.Entry(relatedEntity).State == EntityState.Detached && _context.EntityIsTracked((IIdentifiable)relatedEntity) == false)
                        _context.Entry(relatedEntity).State = EntityState.Unchanged;
                }
                else
                {
                    if (_context.Entry(relationship.Value).State == EntityState.Detached && _context.EntityIsTracked(relationship.Value) == false)
                        _context.Entry(relationship.Value).State = EntityState.Unchanged;
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TId id, TEntity entity)
        {
            var oldEntity = await GetAsync(id);

            if (oldEntity == null)
                return null;

            foreach (var attr in _jsonApiContext.AttributesToUpdate)
                attr.Key.SetValue(oldEntity, attr.Value);

            foreach (var relationship in _jsonApiContext.RelationshipsToUpdate)
                relationship.Key.SetValue(oldEntity, relationship.Value);

            AttachRelationships(oldEntity);

            await _context.SaveChangesAsync();

            return oldEntity;
        }

        /// <inheritdoc />
        public async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            // TODO: it would be better to let this be determined within the relationship attribute...
            // need to think about the right way to do that since HasMany doesn't need to think about this
            // and setting the HasManyThrough.Type to the join type (ArticleTag instead of Tag) for this changes the semantics
            // of the property...
            var typeToUpdate = (relationship is HasManyThroughAttribute hasManyThrough)
                ? hasManyThrough.ThroughType
                : relationship.Type;

            var genericProcessor = _genericProcessorFactory.GetProcessor<IGenericProcessor>(typeof(GenericProcessor<>), typeToUpdate);
            await genericProcessor.UpdateRelationshipsAsync(parent, relationship, relationshipIds);
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = await GetAsync(id);

            if (entity == null)
                return false;

            _dbSet.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName)
        {
            if (string.IsNullOrWhiteSpace(relationshipName)) throw new JsonApiException(400, "Include parameter must not be empty if provided");

            var relationshipChain = relationshipName.Split('.');

            // variables mutated in recursive loop
            // TODO: make recursive method
            string internalRelationshipPath = null;
            var entity = _jsonApiContext.RequestEntity;

            IResourceDefinition logic;

            var requestedRelationship = relationshipChain[0];
            var relationship = entity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == requestedRelationship);
            // need to add this to the typetree so we know what we're dealing with
            if (relationship == null)
            {
                throw new JsonApiException(400, $"Invalid relationship {requestedRelationship} on {entity.EntityName}",
                    $"{entity.EntityName} does not have a relationship named {requestedRelationship}");
            }

            logic = GetLogic(entity.EntityType);


            if (relationship.CanInclude == false)
            {
                throw new JsonApiException(400, $"Including the relationship {requestedRelationship} on {entity.EntityName} is not allowed");
            }

            internalRelationshipPath = (internalRelationshipPath == null) ? relationship.RelationshipPath : $"{internalRelationshipPath}.{relationship.RelationshipPath}";


            //entity = _jsonApiContext.ResourceGraph.GetContextEntity(relationship.Type);


            // if we have logic, we should apply this when getting the relationship
            entities = entities.Include(relationship.RelationshipPath);
            //entites.ArticleTags.Tag -> (internalThroughName).InternalRelationshipname
            if (relationship.IsHasMany)
            {
                // we need to be nested
                var children = GetChildren(entities as IIncludableQueryable<TEntity, object>, relationship, entity, relationshipChain);
            }
            return entities;
        }




        /// <summary>
        /// we Are building the many-to-many relationship here
        /// 
        /// 
        /// if many-to-many we directly go to the one in question: article -> articleTag -> tag (articleTag is skipped!)
        /// 
        /// articles.Include(a => a.ArticleTags)
        ///     .ThenInclude(at => at.Tag)
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="entities"></param>
        /// <param name="relationship"></param>
        /// <returns></returns>
        private IQueryable<TEntity> GetChildren(
            IIncludableQueryable<TEntity, object> entities,
            RelationshipAttribute relationship,
            ContextEntity baseEntity,
            string[] relationshipChain,
            int depth = 0
        )
        {
            var concreteType = typeof(TEntity);
            ContextEntity entity;
            // we are already getting tags here
            var parameters = Expression.Parameter(typeof(TEntity), "model");
            ConstantExpression right;

            IIncludableQueryable<TEntity, object> temp;
            if (relationship.GetType() == typeof(HasManyThroughAttribute)) //assume many-to-many
            {
                var castRelationship = (HasManyThroughAttribute)relationship;
                if (depth > 0)
                {
                    if (depth == 1)
                    {
                        // we are at the throughtable so {ArticleTag}, we need {Tag}
                        // we want to make articleTag => articleTag.Tag

                        // {articleTag}
                        var parameterOne = Expression.Parameter(castRelationship.ThroughType, "articleTag");

                        // {articleTag.Tag}
                        var body = Expression.PropertyOrField(parameterOne, castRelationship.RightProperty.Name);

                        // make expression for this one (with logic!)
                        // REFLECTION
                        var baseType = this.GetType();
                        var method = baseType.GetRuntimeMethods().First(e => e.Name == nameof(MakeThenInclude));
                        var genericMethod = method.MakeGenericMethod(new[] { castRelationship.ThroughType, castRelationship.RightProperty.PropertyType });
                        return  genericMethod.Invoke(this, new object[] { entities, body, parameterOne }) as IIncludableQueryable<TEntity, object>;


                    }

                    throw new NotImplementedException();

                }
                else
                {

                    var relationProperty = concreteType.GetProperty(castRelationship.InternalThroughName);
                    // {article}
                    var parameterOne = Expression.Parameter(typeof(TEntity), "article");
                    if (relationProperty == null)
                        throw new ArgumentException($"'{castRelationship.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

                    var relatedType = relationship.Type;

                    // {article.ArticleTags}
                    var left = Expression.PropertyOrField(parameterOne, castRelationship.InternalThroughName);

                    // we arent doing anything with it, so just return the right side
                    var body = left;
                    // make expression for this one (with logic!)
                    //var includeExpression = Expression.Lambda<Func<TEntity, object>>(body, parameterOne);


                    //temp = entities.Include(includeExpression);

                    var baseType = this.GetType();
                    var method = baseType.GetRuntimeMethods().First(e => e.Name == nameof(MakeInclude));
                    var genericMethod = method.MakeGenericMethod(new[] { castRelationship.RightProperty.PropertyType });
                    temp = (IIncludableQueryable<TEntity, object >) genericMethod.Invoke(this, new object[] { entities, body, parameterOne });

                    // we stil need to do some lovely little ThenIncluding
                    return GetChildren(temp, relationship, baseEntity, relationshipChain, depth: depth + 1);

                }


            }
            else
            {
                var castRelationship = (HasManyThroughAttribute)relationship;
                var relationProperty = concreteType.GetProperty(castRelationship.InternalThroughName);
                // {article}
                var parameterOne = Expression.Parameter(typeof(TEntity), "article");
                if (relationProperty == null)
                    throw new ArgumentException($"'{castRelationship.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

                var relatedType = relationship.Type;

                // {article.ArticleTags}
                var left = Expression.PropertyOrField(parameterOne, castRelationship.InternalThroughName);

                // we arent doing anything with it, so just return the right side
                var body = left;



                if (depth > 0)
                {
                    // make expression for this one (with logic!)
                    var includeExpression = Expression.Lambda<Func<object, object>>(body, parameterOne); //yeah.. object,object


                    temp = entities as IIncludableQueryable<TEntity, object>;
                    temp.ThenInclude(includeExpression);

                }
            }

            return entities;
        }
        /// <summary>
        /// Works. For now.
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <typeparam name="TSecondTo"></typeparam>
        /// <param name="entities"></param>
        /// <param name="body"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IIncludableQueryable<TEntity, TProperty> MakeThenInclude<TTo, TProperty>(IIncludableQueryable<TEntity, TTo> entities, MemberExpression body, ParameterExpression parameter)
        {
            var expression = Expression.Lambda<Func<TTo, TProperty>>(body, parameter);
            return entities.ThenInclude(expression);
        }
        public IIncludableQueryable<TEntity, TProperty> MakeInclude<TProperty>(IQueryable<TEntity> entities, MemberExpression body, ParameterExpression parameter)
        {
            var expression = Expression.Lambda<Func<TEntity, TProperty>>(body, parameter);
            return entities.Include(expression);
        }

        private IIncludableQueryable<TType, object> CustomThenInclude<TType>(IIncludableQueryable<TType, object> temp, Expression<Func<dynamic, object>> includeExpression, Type from, Type to) where TType : class, IIdentifiable
        {
            Type baseType = temp.GetType();
            MethodInfo getMethod = baseType.GetMethod("ThenInclude", BindingFlags.Public);
            MethodInfo genericGet = getMethod.MakeGenericMethod(new[] { from, to });
            return (IIncludableQueryable<TType, object>)genericGet.Invoke(temp, new object[] { includeExpression });
        }

        private IQueryable<TType> CallLogic<TType>(IQueryable<TType> entities, IResourceDefinition resourceDefinition) where TType : class, IIdentifiable
        {
            Type resourceType = resourceDefinition.GetType();
            MethodInfo getMethod = resourceType.GetMethod("OnList", System.Reflection.BindingFlags.Public);
            MethodInfo genericGet = getMethod.MakeGenericMethod(new[] { typeof(TType) });


            return (IQueryable<TType>)genericGet.Invoke(resourceType, new object[] { entities });
        }

        private IResourceDefinition GetLogic(Type model)
        {
            return _genericProcessorFactory.GetProcessor<IResourceDefinition>(typeof(ResourceDefinition<>), model);
        }



        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber)
        {
            if (pageNumber >= 0)
            {
                return await entities.PageForward(pageSize, pageNumber).ToListAsync();
            }

            // since EntityFramework does not support IQueryable.Reverse(), we need to know the number of queried entities
            int numberOfEntities = await this.CountAsync(entities);

            // may be negative
            int virtualFirstIndex = numberOfEntities - pageSize * Math.Abs(pageNumber);
            int numberOfElementsInPage = Math.Min(pageSize, virtualFirstIndex + pageSize);

            return await entities
                    .Skip(virtualFirstIndex)
                    .Take(numberOfElementsInPage)
                    .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(IQueryable<TEntity> entities)
        {
            return (entities is IAsyncEnumerable<TEntity>)
                 ? await entities.CountAsync()
                 : entities.Count();
        }

        /// <inheritdoc />
        public async Task<TEntity> FirstOrDefaultAsync(IQueryable<TEntity> entities)
        {
            return (entities is IAsyncEnumerable<TEntity>)
               ? await entities.FirstOrDefaultAsync()
               : entities.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TEntity>> ToListAsync(IQueryable<TEntity> entities)
        {
            return (entities is IAsyncEnumerable<TEntity>)
                ? await entities.ToListAsync()
                : entities.ToList();
        }
    }
}
