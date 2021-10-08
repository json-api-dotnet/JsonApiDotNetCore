using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    [PublicAPI]
    public class ResourceDefinitionAccessor : IResourceDefinitionAccessor
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IServiceProvider _serviceProvider;

        public ResourceDefinitionAccessor(IResourceGraph resourceGraph, IServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _resourceGraph = resourceGraph;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IImmutableSet<IncludeElementExpression> OnApplyIncludes(ResourceType resourceType, IImmutableSet<IncludeElementExpression> existingIncludes)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyIncludes(existingIncludes);
        }

        /// <inheritdoc />
        public FilterExpression OnApplyFilter(ResourceType resourceType, FilterExpression existingFilter)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyFilter(existingFilter);
        }

        /// <inheritdoc />
        public SortExpression OnApplySort(ResourceType resourceType, SortExpression existingSort)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplySort(existingSort);
        }

        /// <inheritdoc />
        public PaginationExpression OnApplyPagination(ResourceType resourceType, PaginationExpression existingPagination)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyPagination(existingPagination);
        }

        /// <inheritdoc />
        public SparseFieldSetExpression OnApplySparseFieldSet(ResourceType resourceType, SparseFieldSetExpression existingSparseFieldSet)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplySparseFieldSet(existingSparseFieldSet);
        }

        /// <inheritdoc />
        public object GetQueryableHandlerForQueryStringParameter(Type resourceClrType, string parameterName)
        {
            ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));
            ArgumentGuard.NotNullNorEmpty(parameterName, nameof(parameterName));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceClrType);
            dynamic handlers = resourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters();

            return handlers != null && handlers.ContainsKey(parameterName) ? handlers[parameterName] : null;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetMeta(ResourceType resourceType, IIdentifiable resourceInstance)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.GetMeta((dynamic)resourceInstance);
        }

        /// <inheritdoc />
        public async Task OnPrepareWriteAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnPrepareWriteAsync(resource, writeOperation, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IIdentifiable> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
            IIdentifiable rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(leftResource, nameof(leftResource));
            ArgumentGuard.NotNull(hasOneRelationship, nameof(hasOneRelationship));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            return await resourceDefinition.OnSetToOneRelationshipAsync(leftResource, hasOneRelationship, rightResourceId, writeOperation, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
            ISet<IIdentifiable> rightResourceIds, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(leftResource, nameof(leftResource));
            ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnSetToManyRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnAddToRelationshipAsync<TResource, TId>(TId leftResourceId, HasManyAttribute hasManyRelationship,
            ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnAddToRelationshipAsync(leftResourceId, hasManyRelationship, rightResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship,
            ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(leftResource, nameof(leftResource));
            ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnRemoveFromRelationshipAsync(leftResource, hasManyRelationship, rightResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnWritingAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnWritingAsync(resource, writeOperation, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnWriteSucceededAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnWriteSucceededAsync(resource, writeOperation, cancellationToken);
        }

        /// <inheritdoc />
        public void OnDeserialize(IIdentifiable resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(resource.GetType());
            resourceDefinition.OnDeserialize((dynamic)resource);
        }

        /// <inheritdoc />
        public void OnSerialize(IIdentifiable resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(resource.GetType());
            resourceDefinition.OnSerialize((dynamic)resource);
        }

        protected object ResolveResourceDefinition(Type resourceClrType)
        {
            ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
            return ResolveResourceDefinition(resourceType);
        }

        protected virtual object ResolveResourceDefinition(ResourceType resourceType)
        {
            Type resourceDefinitionType = typeof(IResourceDefinition<,>).MakeGenericType(resourceType.ClrType, resourceType.IdentityClrType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }
    }
}
