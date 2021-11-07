using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#pragma warning disable AV2310 // Code block should not contain inline comment
#pragma warning disable AV2318 // Work-tracking TO DO comment should be removed
#pragma warning disable AV2407 // Region should be removed

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Provides the resource services where a NoSQL database such as Cosmos DB or MongoDB
    /// is used as a back-end database.
    /// </summary>
    /// <remarks>
    /// Register <see cref="NoSqlResourceService{TResource,TId}" /> with the service container
    /// as shown in the example.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// public class Startup
    /// {
    ///     public void ConfigureServices(IServiceCollection services)
    ///     {
    ///         services.AddNoSqlResourceServices();
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <typeparam name="TId">The type of the resource Id.</typeparam>
    [PublicAPI]
    public class NoSqlResourceService<TResource, TId> : IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
        where TId : notnull
    {
        protected enum ResourceKind
        {
            Secondary,
            Relationship
        }

        private readonly IResourceRepositoryAccessor _repositoryAccessor;
        private readonly INoSqlQueryLayerComposer _queryLayerComposer;
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly IJsonApiRequest _request;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceGraph _resourceGraph;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;

        private readonly TraceLogWriter<NoSqlResourceService<TResource, TId>> _traceWriter;
        private readonly JsonApiResourceService<TResource, TId> _resourceService;

        public NoSqlResourceService(
            IResourceRepositoryAccessor repositoryAccessor,
            IQueryLayerComposer sqlQueryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceDefinitionAccessor resourceDefinitionAccessor,
            INoSqlQueryLayerComposer queryLayerComposer,
            IResourceGraph resourceGraph,
            IEvaluatedIncludeCache evaluatedIncludeCache)
        {
            _repositoryAccessor = repositoryAccessor;
            _paginationContext = paginationContext;
            _options = options;
            _request = request;
            _resourceChangeTracker = resourceChangeTracker;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;

            _queryLayerComposer = queryLayerComposer;
            _resourceGraph = resourceGraph;
            _evaluatedIncludeCache = evaluatedIncludeCache;

            _traceWriter = new TraceLogWriter<NoSqlResourceService<TResource, TId>>(loggerFactory);

            // Reuse JsonApiResourceService by delegation (rather than inheritance).
            _resourceService = new JsonApiResourceService<TResource, TId>(
                repositoryAccessor, sqlQueryLayerComposer, paginationContext, options,
                loggerFactory, request, resourceChangeTracker, resourceDefinitionAccessor);
        }

        #region Public API

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TResource>> GetAsync(CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart();

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get resources");

            AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

            if (_options.IncludeTotalResourceCount)
            {
                FilterExpression? topFilter = _queryLayerComposer.GetPrimaryFilterFromConstraintsForNoSql(_request.PrimaryResourceType);
                _paginationContext.TotalResourceCount = await _repositoryAccessor.CountAsync(_request.PrimaryResourceType, topFilter, cancellationToken);

                if (_paginationContext.TotalResourceCount == 0)
                {
                    return Array.Empty<TResource>();
                }
            }

            // Compose a query layer and an include expression, where the query layer can be
            // safely used for getting the primary resource because the Include and Projection
            // properties are Empty or null, respectively. The IncludeExpression can be used
            // to fetch the included elements in separate queries.
            var (queryLayer, include) = _queryLayerComposer.ComposeFromConstraintsForNoSql(_request.PrimaryResourceType);

            // Get only the primary resource.
            IReadOnlyCollection<TResource> resources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

            if (queryLayer.Pagination?.PageSize != null && queryLayer.Pagination.PageSize.Value == resources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            // Get the included elements, relying on Entity Framework Core to combine the
            // entities it has fetched.
            await GetIncludedElementsAsync(resources, include, cancellationToken);

            return resources;
        }

        /// <inheritdoc />
        public async Task<TResource> GetAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get single resource");

            return await GetPrimaryResourceByIdWithConstraintsAsync(id, TopFieldSelection.PreserveExisting, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<object?> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                relationshipName
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get secondary resource(s)");

            // Get the primary resource to (1) ensure it exists and (2) retrieve foreign key values as necessary.
            IIdentifiable primary = await GetPrimaryResourceByIdAsync(id, cancellationToken);

            return await GetSecondaryAsync(primary, relationshipName, ResourceKind.Secondary, false, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<object?> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                relationshipName
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get relationship");

            // Get the primary resource to (1) ensure it exists and (2) retrieve foreign key values as necessary.
            IIdentifiable primary = await GetPrimaryResourceByIdAsync(id, cancellationToken);

            return await GetSecondaryAsync(primary, relationshipName, ResourceKind.Relationship, false, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TResource?> CreateAsync(TResource resource, CancellationToken cancellationToken)
        {
            return _resourceService.CreateAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public Task AddToToManyRelationshipAsync(
            TId primaryId,
            string relationshipName,
            ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            return _resourceService.AddToToManyRelationshipAsync(primaryId, relationshipName, secondaryResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResource?> UpdateAsync(TId id, TResource resource, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                resource
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Update resource");

            _resourceChangeTracker.SetRequestAttributeValues(resource);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id, cancellationToken);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceFromDatabase);

            await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);

            // TODO: Revisit. Should we wrap in try-catch clause as JsonApiResourceService?
            await _repositoryAccessor.UpdateAsync(resource, resourceFromDatabase, cancellationToken);

            TResource afterResourceFromDatabase = await GetPrimaryResourceByIdAsync(id, cancellationToken);

            _resourceChangeTracker.SetFinallyStoredAttributeValues(afterResourceFromDatabase);
            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();

            return !hasImplicitChanges ? null! : afterResourceFromDatabase;
        }

        /// <inheritdoc />
        public async Task SetRelationshipAsync(TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftId,
                relationshipName,
                rightValue
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Set relationship");

            AssertHasRelationship(_request.Relationship, relationshipName);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(leftId, cancellationToken);

            await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.SetRelationship, cancellationToken);

            // TODO: Revisit. Should we wrap in try-catch clause as JsonApiResourceService?
            await _repositoryAccessor.SetRelationshipAsync(resourceFromDatabase, rightValue, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            return _resourceService.DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync(
            TId leftId,
            string relationshipName,
            ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftId,
                relationshipName,
                rightResourceIds
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Remove from to-many relationship");

            TResource primaryResource = await GetPrimaryResourceForUpdateAsync(leftId, cancellationToken);

            await _resourceDefinitionAccessor.OnPrepareWriteAsync(primaryResource, WriteOperationKind.RemoveFromRelationship, cancellationToken);

            await _repositoryAccessor.RemoveFromToManyRelationshipAsync(primaryResource, rightResourceIds, cancellationToken);
        }

        #endregion Public API

        #region Implementation

        /// <summary>
        /// Gets the primary resource by ID, specifying only a filter but no other constraints
        /// such as include, page, or fields.
        /// </summary>
        /// <param name="id">The primary resource ID.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <exception cref="ResourceNotFoundException">If the primary resource does not exist.</exception>
        /// <returns>The primary resource with unpopulated navigation properties.</returns>
        protected async Task<TResource> GetPrimaryResourceByIdAsync(TId id, CancellationToken cancellationToken)
        {
            AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

            QueryLayer queryLayer = _queryLayerComposer.ComposeForGetByIdForNoSql(id, _request.PrimaryResourceType);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

            return AssertPrimaryResourceExists(primaryResources.SingleOrDefault());
        }

        /// <summary>
        /// Gets the primary resource by ID, observing all other constraints such as include or fields.
        /// </summary>
        /// <param name="id">The primary resource ID.</param>
        /// <param name="fieldSelection">The <see cref="TopFieldSelection" />.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <exception cref="ResourceNotFoundException">If the primary resource does not exist.</exception>
        /// <returns>
        /// The primary resource with navigation properties populated where such properties represent included resources.
        /// </returns>
        protected async Task<TResource> GetPrimaryResourceByIdWithConstraintsAsync(
            TId id,
            TopFieldSelection fieldSelection,
            CancellationToken cancellationToken)
        {
            AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

            var (primaryLayer, include) = _queryLayerComposer.ComposeForGetByIdWithConstraintsForNoSql(id, _request.PrimaryResourceType, fieldSelection);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

            TResource primaryResource = AssertPrimaryResourceExists(primaryResources.SingleOrDefault());

            await GetIncludedElementsAsync(primaryResources, include, cancellationToken);

            return primaryResource;
        }

        /// <summary>
        /// Gets the primary resource by ID, with all its fields and included resources.
        /// </summary>
        /// <param name="id">The primary resource ID.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns>
        /// The primary resource with navigation properties populated where such properties represent included resources.
        /// </returns>
        protected async Task<TResource> GetPrimaryResourceForUpdateAsync(TId id, CancellationToken cancellationToken)
        {
            AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

            (QueryLayer queryLayer, IncludeExpression include) = _queryLayerComposer.ComposeForUpdateForNoSql(id, _request.PrimaryResourceType);

            TResource primaryResource = AssertPrimaryResourceExists(await _repositoryAccessor.GetForUpdateAsync<TResource>(queryLayer, cancellationToken));

            await GetIncludedElementsAsync(new[]
            {
                primaryResource
            }, include, cancellationToken);

            return primaryResource;
        }

        /// <summary>
        /// For each primary resource in the <paramref name="primaryResources" /> collection, gets
        /// the secondary resources specified in the given <see cref="IncludeExpression" />.
        /// </summary>
        /// <remarks>
        /// An <see cref="IncludeExpression" /> specifies one or more relationships.
        /// </remarks>
        /// <param name="primaryResources">The primary resources.</param>
        /// <param name="includeExpression">The <see cref="IncludeExpression" />.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <exception cref="JsonApiException">
        /// If any <see cref="IncludeElementExpression" /> contained in the <see cref="IncludeExpression"/>
        /// is a nested expression like "first.second".
        /// </exception>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected virtual async Task GetIncludedElementsAsync(
            IReadOnlyCollection<IIdentifiable> primaryResources,
            IncludeExpression includeExpression,
            CancellationToken cancellationToken)
        {
            _evaluatedIncludeCache.Set(includeExpression);

            foreach (var includeElementExpression in includeExpression.Elements)
            {
                await GetIncludedElementAsync(primaryResources, includeElementExpression, cancellationToken);
            }
        }

        /// <summary>
        /// For each primary resource in the <paramref name="primaryResources" /> collection, gets
        /// the secondary resources specified in the given <see cref="IncludeElementExpression" />.
        /// </summary>
        /// <param name="primaryResources">The primary resources.</param>
        /// <param name="includeElementExpression">The <see cref="IncludeElementExpression" />.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <exception cref="JsonApiException">
        /// If the <see cref="IncludeElementExpression" /> is a nested expression like "first.second".
        /// </exception>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected virtual async Task GetIncludedElementAsync(
            IReadOnlyCollection<IIdentifiable> primaryResources,
            IncludeElementExpression includeElementExpression,
            CancellationToken cancellationToken)
        {
            if (includeElementExpression.Children.Any())
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                {
                    Title = "Unsupported expression.",
                    Detail = "Nested include expressions are currently not supported."
                });
            }

            PropertyInfo property = includeElementExpression.Relationship.Property;
            var ownsMany = Attribute.GetCustomAttribute(property, typeof(NoSqlOwnsManyAttribute));

            if (ownsMany is not null)
            {
                return;
            }

            string relationshipName = includeElementExpression.Relationship.PublicName;

            foreach (var primaryResource in primaryResources)
            {
                await GetSecondaryAsync(primaryResource, relationshipName, ResourceKind.Secondary, true, cancellationToken);
            }
        }

        /// <summary>
        /// For to-many relationships, gets the potentially empty collection of related resources.
        /// For to-one relationships, gets zero or one related resource.
        /// </summary>
        /// <param name="primaryResource">The primary resource.</param>
        /// <param name="relationshipName">The name of the relationship between the primary and secondary resources.</param>
        /// <param name="resourceKind"></param>
        /// <param name="isIncluded">Indicates whether the relationship was specified by using "include={relationshipName}".</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <exception cref="JsonApiException">
        /// If the relationship specified by <paramref name="relationshipName" /> does not exist
        /// or does not have a <see cref="NoSqlHasForeignKeyAttribute" />.
        /// </exception>
        /// <returns>
        /// For to-many relationships, an <see cref="IReadOnlyCollection{T}" /> of <see cref="IIdentifiable" />;
        /// for to-one relationships, an <see cref="IIdentifiable" /> or <see langword="null" />.
        /// </returns>
        protected async Task<object?> GetSecondaryAsync(
            IIdentifiable primaryResource,
            string relationshipName,
            ResourceKind resourceKind,
            bool isIncluded,
            CancellationToken cancellationToken)
        {
            // Get the HasMany or HasOne attribute corresponding to the given relationship name.
            ResourceType resourceContext = _resourceGraph.GetResourceType(primaryResource.GetType());

            RelationshipAttribute? relationshipAttribute =
                resourceContext.Relationships.SingleOrDefault(relationship => relationship.PublicName == relationshipName);

            if (relationshipAttribute is null)
            {
                string message = $"The relationship '{relationshipName}' does not exist.";
                _traceWriter.LogMessage(() => message);

                throw new JsonApiException(new ErrorObject(HttpStatusCode.NotFound)
                {
                    Title = "Relationship not found.",
                    Detail = message
                });
            }

            // Check whether the secondary resource is owned by the primary resource.
            PropertyInfo property = relationshipAttribute.Property;
            var ownsMany = Attribute.GetCustomAttribute(property, typeof(NoSqlOwnsManyAttribute));

            if (ownsMany is not null)
            {
                return relationshipAttribute.GetValue(primaryResource);
            }

            // Get the HasForeignKey attribute corresponding to the relationship, if any.
            var foreignKeyAttribute = (NoSqlHasForeignKeyAttribute?)Attribute.GetCustomAttribute(
                relationshipAttribute.Property, typeof(NoSqlHasForeignKeyAttribute));

            if (foreignKeyAttribute is null)
            {
                string message = $"No foreign key is specified for the relationship '{relationshipName}'.";
                _traceWriter.LogMessage(() => message);

                throw new JsonApiException(new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "Invalid resource definition.",
                    Detail = message
                });
            }

            // Finally, get the secondary resource or resources based on the target (right) type,
            // foreign key, and the information on whether the primary resource is on the dependent
            // side of the foreign key relationship.
            ResourceType type = relationshipAttribute.RightType;
            string foreignKey = foreignKeyAttribute.PropertyName;
            string stringId = primaryResource.StringId!;
            bool isDependent = foreignKeyAttribute.IsDependent;

            return relationshipAttribute switch
            {
                HasManyAttribute => await GetManySecondaryResourcesAsync(type, foreignKey, stringId, resourceKind, isIncluded, cancellationToken),

                HasOneAttribute when isDependent => await GetOneSecondaryResourceAsync(type, nameof(IIdentifiable<TId>.Id),
                    GetStringValue(primaryResource, foreignKey), resourceKind, isIncluded, cancellationToken),

                HasOneAttribute when !isDependent => throw new JsonApiException(new ErrorObject(HttpStatusCode.NotImplemented)
                {
                    Title = "Unsupported relationship.",
                    Detail = "One-to-one relationships are not yet supported."
                }),

                _ => throw new JsonApiException(new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "Invalid relationship.",
                    Detail = $"The relationship '{relationshipName}' is invalid."
                })
            };
        }

        /// <summary>
        /// For to-one relationships (e.g., Parent), gets the secondary resource,
        /// if any, filtered by "equals({propertyName},'{propertyValue}')".
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="propertyName">The name of the property used to filter resources, e.g., "Id".</param>
        /// <param name="propertyValue">The value of the property used to filter resources, e.g., "e0bd6fe1-889e-4a06-84f8-5cf2e8d58466".</param>
        /// <param name="resourceKind"></param>
        /// <param name="isIncluded"></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns>
        /// The <see cref="IIdentifiable" />, if it exists, or <see langword="null" />.
        /// </returns>
        protected async Task<IIdentifiable?> GetOneSecondaryResourceAsync(
            ResourceType resourceType,
            string propertyName,
            string? propertyValue,
            ResourceKind resourceKind,
            bool isIncluded,
            CancellationToken cancellationToken)
        {
            if (propertyValue is null)
            {
                return null;
            }

            IReadOnlyCollection<IIdentifiable> items = await GetManySecondaryResourcesAsync(
                resourceType, propertyName, propertyValue, resourceKind, isIncluded, cancellationToken);

            return items.SingleOrDefault();
        }

        /// <summary>
        /// For to-many relationships (e.g., Children), gets the collection of secondary resources, filtered
        /// by the filter expressions provided in the request and by equals({propertyName},'{propertyValue}').
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="propertyName">The name of the property used to filter resources, e.g., "ParentId".</param>
        /// <param name="propertyValue">The value of the property used to filter resources, e.g., "e0bd6fe1-889e-4a06-84f8-5cf2e8d58466".</param>
        /// <param name="resourceKind"></param>
        /// <param name="isIncluded">Indicates whether or not the secondary resource is included by way of an include expression.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns>The potentially empty collection of secondary resources.</returns>
        protected async Task<IReadOnlyCollection<IIdentifiable>> GetManySecondaryResourcesAsync(
            ResourceType resourceType,
            string propertyName,
            string propertyValue,
            ResourceKind resourceKind,
            bool isIncluded,
            CancellationToken cancellationToken)
        {
            var (queryLayer, include) = _queryLayerComposer.ComposeFromConstraintsForNoSql(resourceType, propertyName, propertyValue, isIncluded);

            IReadOnlyCollection<IIdentifiable> items = await _repositoryAccessor.GetAsync(resourceType, queryLayer, cancellationToken);

            if (resourceKind != ResourceKind.Relationship && !isIncluded)
            {
                await GetIncludedElementsAsync(items, include, cancellationToken);
            }

            return items;
        }

        [AssertionMethod]
        private TResource AssertPrimaryResourceExists([SysNotNull] TResource? resource)
        {
            AssertPrimaryResourceTypeInJsonApiRequestIsNotNull(_request.PrimaryResourceType);

            return resource ?? throw new ResourceNotFoundException(_request.PrimaryId!, _request.PrimaryResourceType.PublicName);
        }

        [AssertionMethod]
        private void AssertHasRelationship([SysNotNull] RelationshipAttribute? relationship, string name)
        {
            if (relationship is null)
            {
                throw new RelationshipNotFoundException(name, _request.PrimaryResourceType!.PublicName);
            }
        }

        [AssertionMethod]
        private static void AssertPrimaryResourceTypeInJsonApiRequestIsNotNull([SysNotNull] ResourceType? resourceType)
        {
            if (resourceType is null)
            {
                throw new InvalidOperationException(
                    $"Expected {nameof(IJsonApiRequest)}.{nameof(IJsonApiRequest.PrimaryResourceType)} not to be null at this point.");
            }
        }

        [AssertionMethod]
        private static void AssertRelationshipInJsonApiRequestIsNotNull([SysNotNull] RelationshipAttribute? relationship)
        {
            if (relationship is null)
            {
                throw new InvalidOperationException($"Expected {nameof(IJsonApiRequest)}.{nameof(IJsonApiRequest.Relationship)} not to be null at this point.");
            }
        }

        /// <summary>
        /// Gets the <see cref="string" /> value of the named property.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The <see cref="string" /> value of the named property.</returns>
        protected string? GetStringValue(object resource, string propertyName)
        {
            Type type = resource.GetType();
            PropertyInfo? property = type.GetProperty(propertyName);

            if (property is null)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "Invalid property.",
                    Detail = $"The '{type.Name}' type does not have a '{propertyName}' property."
                });
            }

            return property.GetValue(resource)?.ToString();
        }

        #endregion Implementation
    }
}
