using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses Entity Framework Core.
    /// </summary>
    [PublicAPI]
    public class EntityFrameworkCoreRepository<TResource, TId> : IResourceRepository<TResource, TId>, IRepositorySupportsTransaction
        where TResource : class, IIdentifiable<TId>
    {
        private readonly CollectionConverter _collectionConverter = new CollectionConverter();
        private readonly ITargetedFields _targetedFields;
        private readonly DbContext _dbContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>> _traceWriter;

        /// <inheritdoc />
        public virtual Guid? TransactionId => _dbContext.Database.CurrentTransaction?.TransactionId;

        public EntityFrameworkCoreRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
        {
            ArgumentGuard.NotNull(contextResolver, nameof(contextResolver));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));

            _targetedFields = targetedFields;
            _resourceGraph = resourceGraph;
            _resourceFactory = resourceFactory;
            _constraintProviders = constraintProviders;
            _dbContext = contextResolver.GetContext();
            _traceWriter = new TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>>(loggerFactory);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                layer
            });

            ArgumentGuard.NotNull(layer, nameof(layer));

            IQueryable<TResource> query = ApplyQueryLayer(layer);
            return await query.ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                topFilter
            });

            ResourceContext resourceContext = _resourceGraph.GetResourceContext<TResource>();

            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            IQueryable<TResource> query = ApplyQueryLayer(layer);
            return await query.CountAsync(cancellationToken);
        }

        protected virtual IQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            _traceWriter.LogMethodStart(new
            {
                layer
            });

            ArgumentGuard.NotNull(layer, nameof(layer));

            QueryLayer rewrittenLayer = layer;

            if (EntityFrameworkCoreSupport.Version.Major < 5)
            {
                var writer = new MemoryLeakDetectionBugRewriter();
                rewrittenLayer = writer.Rewrite(layer);
            }

            IQueryable<TResource> source = GetAll();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            QueryableHandlerExpression[] queryableHandlers = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Where(expressionInScope => expressionInScope.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<QueryableHandlerExpression>()
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            foreach (QueryableHandlerExpression queryableHandler in queryableHandlers)
            {
                source = queryableHandler.Apply(source);
            }

            var nameFactory = new LambdaParameterNameFactory();

            var builder = new QueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory, _resourceGraph,
                _dbContext.Model);

            Expression expression = builder.ApplyQuery(rewrittenLayer);
            return source.Provider.CreateQuery<TResource>(expression);
        }

        protected virtual IQueryable<TResource> GetAll()
        {
            return _dbContext.Set<TResource>();
        }

        /// <inheritdoc />
        public virtual Task<TResource> GetForCreateAsync(TId id, CancellationToken cancellationToken)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return Task.FromResult(resource);
        }

        /// <inheritdoc />
        public virtual async Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                resourceFromRequest,
                resourceForDatabase
            });

            ArgumentGuard.NotNull(resourceFromRequest, nameof(resourceFromRequest));
            ArgumentGuard.NotNull(resourceForDatabase, nameof(resourceForDatabase));

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);

            foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
            {
                object rightResources = relationship.GetValue(resourceFromRequest);
                await UpdateRelationshipAsync(relationship, resourceForDatabase, rightResources, collector, cancellationToken);
            }

            foreach (AttrAttribute attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
            }

            DbSet<TResource> dbSet = _dbContext.Set<TResource>();
            await dbSet.AddAsync(resourceForDatabase, cancellationToken);

            await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<TResource> resources = await GetAsync(queryLayer, cancellationToken);
            return resources.FirstOrDefault();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                resourceFromRequest,
                resourceFromDatabase
            });

            ArgumentGuard.NotNull(resourceFromRequest, nameof(resourceFromRequest));
            ArgumentGuard.NotNull(resourceFromDatabase, nameof(resourceFromDatabase));

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);

            foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
            {
                object rightResources = relationship.GetValue(resourceFromRequest);

                AssertIsNotClearingRequiredRelationship(relationship, resourceFromDatabase, rightResources);

                await UpdateRelationshipAsync(relationship, resourceFromDatabase, rightResources, collector, cancellationToken);
            }

            foreach (AttrAttribute attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceFromDatabase, attribute.GetValue(resourceFromRequest));
            }

            await SaveChangesAsync(cancellationToken);
        }

        protected void AssertIsNotClearingRequiredRelationship(RelationshipAttribute relationship, TResource leftResource, object rightValue)
        {
            bool relationshipIsRequired = false;

            if (!(relationship is HasManyThroughAttribute))
            {
                INavigation navigation = TryGetNavigation(relationship);
                relationshipIsRequired = navigation?.ForeignKey?.IsRequired ?? false;
            }

            bool relationshipIsBeingCleared = relationship is HasOneAttribute
                ? rightValue == null
                : IsToManyRelationshipBeingCleared(relationship, leftResource, rightValue);

            if (relationshipIsRequired && relationshipIsBeingCleared)
            {
                string resourceType = _resourceGraph.GetResourceContext<TResource>().PublicName;
                throw new CannotClearRequiredRelationshipException(relationship.PublicName, leftResource.StringId, resourceType);
            }
        }

        private bool IsToManyRelationshipBeingCleared(RelationshipAttribute relationship, TResource leftResource, object valueToAssign)
        {
            ICollection<IIdentifiable> newRightResourceIds = _collectionConverter.ExtractResources(valueToAssign);

            object existingRightValue = relationship.GetValue(leftResource);

            HashSet<IIdentifiable> existingRightResourceIds =
                _collectionConverter.ExtractResources(existingRightValue).ToHashSet(IdentifiableComparer.Instance);

            existingRightResourceIds.ExceptWith(newRightResourceIds);

            return existingRightResourceIds.Any();
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            TResource resource = collector.CreateForId<TResource, TId>(id);

            foreach (RelationshipAttribute relationship in _resourceGraph.GetRelationships<TResource>())
            {
                // Loads the data of the relationship, if in EF Core it is configured in such a way that loading the related
                // entities into memory is required for successfully executing the selected deletion behavior.
                if (RequiresLoadOfRelationshipForDeletion(relationship))
                {
                    NavigationEntry navigation = GetNavigationEntry(resource, relationship);
                    await navigation.LoadAsync(cancellationToken);
                }
            }

            _dbContext.Remove(resource);

            await SaveChangesAsync(cancellationToken);
        }

        private NavigationEntry GetNavigationEntry(TResource resource, RelationshipAttribute relationship)
        {
            EntityEntry<TResource> entityEntry = _dbContext.Entry(resource);

            switch (relationship)
            {
                case HasOneAttribute hasOneRelationship:
                {
                    return entityEntry.Reference(hasOneRelationship.Property.Name);
                }
                case HasManyAttribute hasManyRelationship:
                {
                    return entityEntry.Collection(hasManyRelationship.Property.Name);
                }
                default:
                {
                    throw new InvalidOperationException($"Unknown relationship type '{relationship.GetType().Name}'.");
                }
            }
        }

        private bool RequiresLoadOfRelationshipForDeletion(RelationshipAttribute relationship)
        {
            INavigation navigation = TryGetNavigation(relationship);
            bool isClearOfForeignKeyRequired = navigation?.ForeignKey.DeleteBehavior == DeleteBehavior.ClientSetNull;

            bool hasForeignKeyAtLeftSide = HasForeignKeyAtLeftSide(relationship);

            return isClearOfForeignKeyRequired && !hasForeignKeyAtLeftSide;
        }

        private INavigation TryGetNavigation(RelationshipAttribute relationship)
        {
            IEntityType entityType = _dbContext.Model.FindEntityType(typeof(TResource));
            return entityType?.FindNavigation(relationship.Property.Name);
        }

        private bool HasForeignKeyAtLeftSide(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute)
            {
                INavigation navigation = TryGetNavigation(relationship);
                return navigation?.IsDependentToPrincipal() ?? false;
            }

            return false;
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                primaryResource,
                secondaryResourceIds
            });

            RelationshipAttribute relationship = _targetedFields.Relationships.Single();

            AssertIsNotClearingRequiredRelationship(relationship, primaryResource, secondaryResourceIds);

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            await UpdateRelationshipAsync(relationship, primaryResource, secondaryResourceIds, collector, cancellationToken);

            await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task AddToToManyRelationshipAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                primaryId,
                secondaryResourceIds
            });

            ArgumentGuard.NotNull(secondaryResourceIds, nameof(secondaryResourceIds));

            RelationshipAttribute relationship = _targetedFields.Relationships.Single();

            if (secondaryResourceIds.Any())
            {
                using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
                TResource primaryResource = collector.CreateForId<TResource, TId>(primaryId);

                await UpdateRelationshipAsync(relationship, primaryResource, secondaryResourceIds, collector, cancellationToken);

                await SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual async Task RemoveFromToManyRelationshipAsync(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                primaryResource,
                secondaryResourceIds
            });

            ArgumentGuard.NotNull(secondaryResourceIds, nameof(secondaryResourceIds));

            var relationship = (HasManyAttribute)_targetedFields.Relationships.Single();

            object rightValue = relationship.GetValue(primaryResource);

            HashSet<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);
            rightResourceIds.ExceptWith(secondaryResourceIds);

            AssertIsNotClearingRequiredRelationship(relationship, primaryResource, rightResourceIds);

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            await UpdateRelationshipAsync(relationship, primaryResource, rightResourceIds, collector, cancellationToken);

            await SaveChangesAsync(cancellationToken);
        }

        protected async Task UpdateRelationshipAsync(RelationshipAttribute relationship, TResource leftResource, object valueToAssign,
            PlaceholderResourceCollector collector, CancellationToken cancellationToken)
        {
            object trackedValueToAssign = EnsureRelationshipValueToAssignIsTracked(valueToAssign, relationship.Property.PropertyType, collector);

            if (RequireLoadOfInverseRelationship(relationship, trackedValueToAssign))
            {
                EntityEntry entityEntry = _dbContext.Entry(trackedValueToAssign);
                string inversePropertyName = relationship.InverseNavigationProperty.Name;

                await entityEntry.Reference(inversePropertyName).LoadAsync(cancellationToken);
            }

            relationship.SetValue(leftResource, trackedValueToAssign);
        }

        private object EnsureRelationshipValueToAssignIsTracked(object rightValue, Type relationshipPropertyType, PlaceholderResourceCollector collector)
        {
            if (rightValue == null)
            {
                return null;
            }

            ICollection<IIdentifiable> rightResources = _collectionConverter.ExtractResources(rightValue);
            IIdentifiable[] rightResourcesTracked = rightResources.Select(collector.CaptureExisting).ToArray();

            return rightValue is IEnumerable
                ? (object)_collectionConverter.CopyToTypedCollection(rightResourcesTracked, relationshipPropertyType)
                : rightResourcesTracked.Single();
        }

        private bool RequireLoadOfInverseRelationship(RelationshipAttribute relationship, object trackedValueToAssign)
        {
            // See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
            return trackedValueToAssign != null && relationship.InverseNavigationProperty != null && IsOneToOneRelationship(relationship);
        }

        private bool IsOneToOneRelationship(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                Type elementType = _collectionConverter.TryGetCollectionElementType(hasOneRelationship.InverseNavigationProperty.PropertyType);
                return elementType == null;
            }

            return false;
        }

        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                if (_dbContext.Database.CurrentTransaction != null)
                {
                    // The ResourceService calling us needs to run additional SQL queries after an aborted transaction,
                    // to determine error cause. This fails when a failed transaction is still in progress.
                    await _dbContext.Database.CurrentTransaction.RollbackAsync(cancellationToken);
                }

                throw new DataStoreUpdateException(exception);
            }
        }
    }

    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses Entity Framework Core.
    /// </summary>
    [PublicAPI]
    public class EntityFrameworkCoreRepository<TResource> : EntityFrameworkCoreRepository<TResource, int>, IResourceRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public EntityFrameworkCoreRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
        }
    }
}
