using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
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
        private readonly CollectionConverter _collectionConverter = new();
        private readonly ITargetedFields _targetedFields;
        private readonly DbContext _dbContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>> _traceWriter;

        /// <inheritdoc />
        public virtual string? TransactionId => _dbContext.Database.CurrentTransaction?.TransactionId.ToString();

        public EntityFrameworkCoreRepository(ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
            IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
            ArgumentGuard.NotNull(dbContextResolver, nameof(dbContextResolver));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

            _targetedFields = targetedFields;
            _dbContext = dbContextResolver.GetContext();
            _resourceGraph = resourceGraph;
            _resourceFactory = resourceFactory;
            _constraintProviders = constraintProviders;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _traceWriter = new TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>>(loggerFactory);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                queryLayer
            });

            ArgumentGuard.NotNull(queryLayer, nameof(queryLayer));

            using (CodeTimingSessionManager.Current.Measure("Repository - Get resource(s)"))
            {
                IQueryable<TResource> query = ApplyQueryLayer(queryLayer);

                using (CodeTimingSessionManager.Current.Measure("Execute SQL (data)", MeasurementSettings.ExcludeDatabaseInPercentages))
                {
                    return await query.ToListAsync(cancellationToken);
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(FilterExpression? topFilter, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                topFilter
            });

            using (CodeTimingSessionManager.Current.Measure("Repository - Count resources"))
            {
                ResourceType resourceType = _resourceGraph.GetResourceType<TResource>();

                var layer = new QueryLayer(resourceType)
                {
                    Filter = topFilter
                };

                IQueryable<TResource> query = ApplyQueryLayer(layer);

                using (CodeTimingSessionManager.Current.Measure("Execute SQL (count)", MeasurementSettings.ExcludeDatabaseInPercentages))
                {
                    return await query.CountAsync(cancellationToken);
                }
            }
        }

        protected virtual IQueryable<TResource> ApplyQueryLayer(QueryLayer queryLayer)
        {
            _traceWriter.LogMethodStart(new
            {
                queryLayer
            });

            ArgumentGuard.NotNull(queryLayer, nameof(queryLayer));

            using (CodeTimingSessionManager.Current.Measure("Convert QueryLayer to System.Expression"))
            {
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

                var builder = new QueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory, _dbContext.Model);

                Expression expression = builder.ApplyQuery(queryLayer);

                using (CodeTimingSessionManager.Current.Measure("Convert System.Expression to IQueryable"))
                {
                    return source.Provider.CreateQuery<TResource>(expression);
                }
            }
        }

        protected virtual IQueryable<TResource> GetAll()
        {
            return _dbContext.Set<TResource>();
        }

        /// <inheritdoc />
        public virtual Task<TResource> GetForCreateAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

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

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Create resource");

            foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
            {
                object? rightValue = relationship.GetValue(resourceFromRequest);

                object? rightValueEvaluated = await VisitSetRelationshipAsync(resourceForDatabase, relationship, rightValue, WriteOperationKind.CreateResource,
                    cancellationToken);

                await UpdateRelationshipAsync(relationship, resourceForDatabase, rightValueEvaluated, cancellationToken);
            }

            foreach (AttrAttribute attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceForDatabase, attribute.GetValue(resourceFromRequest));
            }

            await _resourceDefinitionAccessor.OnWritingAsync(resourceForDatabase, WriteOperationKind.CreateResource, cancellationToken);

            DbSet<TResource> dbSet = _dbContext.Set<TResource>();
            await dbSet.AddAsync(resourceForDatabase, cancellationToken);

            await SaveChangesAsync(cancellationToken);

            await _resourceDefinitionAccessor.OnWriteSucceededAsync(resourceForDatabase, WriteOperationKind.CreateResource, cancellationToken);

            _dbContext.ResetChangeTracker();
        }

        private async Task<object?> VisitSetRelationshipAsync(TResource leftResource, RelationshipAttribute relationship, object? rightValue,
            WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                return await _resourceDefinitionAccessor.OnSetToOneRelationshipAsync(leftResource, hasOneRelationship, (IIdentifiable?)rightValue,
                    writeOperation, cancellationToken);
            }

            if (relationship is HasManyAttribute hasManyRelationship)
            {
                HashSet<IIdentifiable> rightResourceIdSet = _collectionConverter.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);

                await _resourceDefinitionAccessor.OnSetToManyRelationshipAsync(leftResource, hasManyRelationship, rightResourceIdSet, writeOperation,
                    cancellationToken);

                return rightResourceIdSet;
            }

            return rightValue;
        }

        /// <inheritdoc />
        public virtual async Task<TResource?> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                queryLayer
            });

            ArgumentGuard.NotNull(queryLayer, nameof(queryLayer));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Get resource for update");

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

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Update resource");

            foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
            {
                object? rightValue = relationship.GetValue(resourceFromRequest);

                object? rightValueEvaluated = await VisitSetRelationshipAsync(resourceFromDatabase, relationship, rightValue, WriteOperationKind.UpdateResource,
                    cancellationToken);

                AssertIsNotClearingRequiredToOneRelationship(relationship, resourceFromDatabase, rightValueEvaluated);

                await UpdateRelationshipAsync(relationship, resourceFromDatabase, rightValueEvaluated, cancellationToken);
            }

            foreach (AttrAttribute attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceFromDatabase, attribute.GetValue(resourceFromRequest));
            }

            await _resourceDefinitionAccessor.OnWritingAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);

            await SaveChangesAsync(cancellationToken);

            await _resourceDefinitionAccessor.OnWriteSucceededAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);

            _dbContext.ResetChangeTracker();
        }

        protected void AssertIsNotClearingRequiredToOneRelationship(RelationshipAttribute relationship, TResource leftResource, object? rightValue)
        {
            if (relationship is HasOneAttribute)
            {
                INavigation? navigation = GetNavigation(relationship);
                bool isRelationshipRequired = navigation?.ForeignKey?.IsRequired ?? false;

                bool isClearingRelationship = rightValue == null;

                if (isRelationshipRequired && isClearingRelationship)
                {
                    string resourceName = _resourceGraph.GetResourceType<TResource>().PublicName;
                    throw new CannotClearRequiredRelationshipException(relationship.PublicName, leftResource.StringId!, resourceName);
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Delete resource");

            // This enables OnWritingAsync() to fetch the resource, which adds it to the change tracker.
            // If so, we'll reuse the tracked resource instead of this placeholder resource.
            var placeholderResource = _resourceFactory.CreateInstance<TResource>();
            placeholderResource.Id = id;

            await _resourceDefinitionAccessor.OnWritingAsync(placeholderResource, WriteOperationKind.DeleteResource, cancellationToken);

            var resourceTracked = (TResource)_dbContext.GetTrackedOrAttach(placeholderResource);

            foreach (RelationshipAttribute relationship in _resourceGraph.GetResourceType<TResource>().Relationships)
            {
                // Loads the data of the relationship, if in Entity Framework Core it is configured in such a way that loading
                // the related entities into memory is required for successfully executing the selected deletion behavior.
                if (RequiresLoadOfRelationshipForDeletion(relationship))
                {
                    NavigationEntry navigation = GetNavigationEntry(resourceTracked, relationship);
                    await navigation.LoadAsync(cancellationToken);
                }
            }

            _dbContext.Remove(resourceTracked);

            await SaveChangesAsync(cancellationToken);

            await _resourceDefinitionAccessor.OnWriteSucceededAsync(resourceTracked, WriteOperationKind.DeleteResource, cancellationToken);
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
            INavigation? navigation = GetNavigation(relationship);
            bool isClearOfForeignKeyRequired = navigation?.ForeignKey.DeleteBehavior == DeleteBehavior.ClientSetNull;

            bool hasForeignKeyAtLeftSide = HasForeignKeyAtLeftSide(relationship, navigation);

            return isClearOfForeignKeyRequired && !hasForeignKeyAtLeftSide;
        }

        private INavigation? GetNavigation(RelationshipAttribute relationship)
        {
            IEntityType entityType = _dbContext.Model.FindEntityType(typeof(TResource));
            return entityType?.FindNavigation(relationship.Property.Name);
        }

        private bool HasForeignKeyAtLeftSide(RelationshipAttribute relationship, INavigation? navigation)
        {
            return relationship is HasOneAttribute && navigation is { IsOnDependent: true };
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TResource leftResource, object? rightValue, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftResource,
                rightValue
            });

            ArgumentGuard.NotNull(leftResource, nameof(leftResource));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Set relationship");

            RelationshipAttribute relationship = _targetedFields.Relationships.Single();

            object? rightValueEvaluated =
                await VisitSetRelationshipAsync(leftResource, relationship, rightValue, WriteOperationKind.SetRelationship, cancellationToken);

            AssertIsNotClearingRequiredToOneRelationship(relationship, leftResource, rightValueEvaluated);

            await UpdateRelationshipAsync(relationship, leftResource, rightValueEvaluated, cancellationToken);

            await _resourceDefinitionAccessor.OnWritingAsync(leftResource, WriteOperationKind.SetRelationship, cancellationToken);

            await SaveChangesAsync(cancellationToken);

            await _resourceDefinitionAccessor.OnWriteSucceededAsync(leftResource, WriteOperationKind.SetRelationship, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task AddToToManyRelationshipAsync(TId leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftId,
                rightResourceIds
            });

            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Add to to-many relationship");

            var relationship = (HasManyAttribute)_targetedFields.Relationships.Single();

            await _resourceDefinitionAccessor.OnAddToRelationshipAsync<TResource, TId>(leftId, relationship, rightResourceIds, cancellationToken);

            if (rightResourceIds.Any())
            {
                var leftPlaceholderResource = _resourceFactory.CreateInstance<TResource>();
                leftPlaceholderResource.Id = leftId;

                var leftResourceTracked = (TResource)_dbContext.GetTrackedOrAttach(leftPlaceholderResource);

                await UpdateRelationshipAsync(relationship, leftResourceTracked, rightResourceIds, cancellationToken);

                await _resourceDefinitionAccessor.OnWritingAsync(leftResourceTracked, WriteOperationKind.AddToRelationship, cancellationToken);

                await SaveChangesAsync(cancellationToken);

                await _resourceDefinitionAccessor.OnWriteSucceededAsync(leftResourceTracked, WriteOperationKind.AddToRelationship, cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual async Task RemoveFromToManyRelationshipAsync(TResource leftResource, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftResource,
                rightResourceIds
            });

            ArgumentGuard.NotNull(leftResource, nameof(leftResource));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Remove from to-many relationship");

            var relationship = (HasManyAttribute)_targetedFields.Relationships.Single();
            HashSet<IIdentifiable> rightResourceIdsToRemove = rightResourceIds.ToHashSet(IdentifiableComparer.Instance);

            await _resourceDefinitionAccessor.OnRemoveFromRelationshipAsync(leftResource, relationship, rightResourceIdsToRemove, cancellationToken);

            if (rightResourceIdsToRemove.Any())
            {
                var leftResourceTracked = (TResource)_dbContext.GetTrackedOrAttach(leftResource);

                // Make Entity Framework Core believe any additional resources added from ResourceDefinition already exist in database.
                IIdentifiable[] extraResourceIdsToRemove = rightResourceIdsToRemove.Where(rightId => !rightResourceIds.Contains(rightId)).ToArray();

                object? rightValueStored = relationship.GetValue(leftResource);

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                IIdentifiable[] rightResourceIdsStored = _collectionConverter
                    .ExtractResources(rightValueStored)
                    .Concat(extraResourceIdsToRemove)
                    .Select(rightResource => _dbContext.GetTrackedOrAttach(rightResource))
                    .ToArray();

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                rightValueStored = _collectionConverter.CopyToTypedCollection(rightResourceIdsStored, relationship.Property.PropertyType);
                relationship.SetValue(leftResource, rightValueStored);

                MarkRelationshipAsLoaded(leftResource, relationship);

                HashSet<IIdentifiable> rightResourceIdsToStore = rightResourceIdsStored.ToHashSet(IdentifiableComparer.Instance);
                rightResourceIdsToStore.ExceptWith(rightResourceIdsToRemove);

                AssertIsNotClearingRequiredToOneRelationship(relationship, leftResourceTracked, rightResourceIdsToStore);

                await UpdateRelationshipAsync(relationship, leftResourceTracked, rightResourceIdsToStore, cancellationToken);

                await _resourceDefinitionAccessor.OnWritingAsync(leftResourceTracked, WriteOperationKind.RemoveFromRelationship, cancellationToken);

                await SaveChangesAsync(cancellationToken);

                await _resourceDefinitionAccessor.OnWriteSucceededAsync(leftResourceTracked, WriteOperationKind.RemoveFromRelationship, cancellationToken);
            }
        }

        private void MarkRelationshipAsLoaded(TResource leftResource, RelationshipAttribute relationship)
        {
            EntityEntry<TResource> leftEntry = _dbContext.Entry(leftResource);
            CollectionEntry rightCollectionEntry = leftEntry.Collection(relationship.Property.Name);
            rightCollectionEntry.IsLoaded = true;
        }

        protected async Task UpdateRelationshipAsync(RelationshipAttribute relationship, TResource leftResource, object? valueToAssign,
            CancellationToken cancellationToken)
        {
            object? trackedValueToAssign = EnsureRelationshipValueToAssignIsTracked(valueToAssign, relationship.Property.PropertyType);

            if (RequireLoadOfInverseRelationship(relationship, trackedValueToAssign))
            {
                EntityEntry entityEntry = _dbContext.Entry(trackedValueToAssign);
                string inversePropertyName = relationship.InverseNavigationProperty!.Name;

                await entityEntry.Reference(inversePropertyName).LoadAsync(cancellationToken);
            }

            relationship.SetValue(leftResource, trackedValueToAssign);
        }

        private object? EnsureRelationshipValueToAssignIsTracked(object? rightValue, Type relationshipPropertyType)
        {
            if (rightValue == null)
            {
                return null;
            }

            ICollection<IIdentifiable> rightResources = _collectionConverter.ExtractResources(rightValue);
            IIdentifiable[] rightResourcesTracked = rightResources.Select(rightResource => _dbContext.GetTrackedOrAttach(rightResource)).ToArray();

            return rightValue is IEnumerable
                ? _collectionConverter.CopyToTypedCollection(rightResourcesTracked, relationshipPropertyType)
                : rightResourcesTracked.Single();
        }

        private bool RequireLoadOfInverseRelationship(RelationshipAttribute relationship, [NotNullWhen(true)] object? trackedValueToAssign)
        {
            // See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
            return trackedValueToAssign != null && relationship is HasOneAttribute { IsOneToOne: true };
        }

        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using IDisposable _ = CodeTimingSessionManager.Current.Measure("Persist EF Core changes", MeasurementSettings.ExcludeDatabaseInPercentages);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException)
            {
                if (_dbContext.Database.CurrentTransaction != null)
                {
                    // The ResourceService calling us needs to run additional SQL queries after an aborted transaction,
                    // to determine error cause. This fails when a failed transaction is still in progress.
                    await _dbContext.Database.CurrentTransaction.RollbackAsync(cancellationToken);
                }

                _dbContext.ResetChangeTracker();

                throw new DataStoreUpdateException(exception);
            }
        }
    }
}
