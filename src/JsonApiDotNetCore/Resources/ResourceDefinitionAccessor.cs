using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    [PublicAPI]
    public class ResourceDefinitionAccessor : IResourceDefinitionAccessor
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IServiceProvider _serviceProvider;

        public ResourceDefinitionAccessor(IResourceContextProvider resourceContextProvider, IServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _resourceContextProvider = resourceContextProvider;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(Type resourceType, IReadOnlyCollection<IncludeElementExpression> existingIncludes)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyIncludes(existingIncludes);
        }

        /// <inheritdoc />
        public FilterExpression OnApplyFilter(Type resourceType, FilterExpression existingFilter)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyFilter(existingFilter);
        }

        /// <inheritdoc />
        public SortExpression OnApplySort(Type resourceType, SortExpression existingSort)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplySort(existingSort);
        }

        /// <inheritdoc />
        public PaginationExpression OnApplyPagination(Type resourceType, PaginationExpression existingPagination)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyPagination(existingPagination);
        }

        /// <inheritdoc />
        public SparseFieldSetExpression OnApplySparseFieldSet(Type resourceType, SparseFieldSetExpression existingSparseFieldSet)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplySparseFieldSet(existingSparseFieldSet);
        }

        /// <inheritdoc />
        public object GetQueryableHandlerForQueryStringParameter(Type resourceType, string parameterName)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));
            ArgumentGuard.NotNullNorEmpty(parameterName, nameof(parameterName));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            dynamic handlers = resourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters();

            return handlers != null && handlers.ContainsKey(parameterName) ? handlers[parameterName] : null;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetMeta(Type resourceType, IIdentifiable resourceInstance)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.GetMeta((dynamic)resourceInstance);
        }

        /// <inheritdoc />
        public async Task OnInitializeResourceAsync<TResource>(TResource resource, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnInitializeResourceAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnBeforeCreateResourceAsync<TResource>(TResource resource, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnBeforeCreateResourceAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnAfterCreateResourceAsync<TResource>(TResource resource, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnAfterCreateResourceAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnAfterGetForUpdateResourceAsync<TResource>(TResource resource, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnAfterGetForUpdateResourceAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnBeforeUpdateResourceAsync<TResource>(TResource resource, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnBeforeUpdateResourceAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnAfterUpdateResourceAsync<TResource>(TResource resource, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnAfterUpdateResourceAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnBeforeDeleteResourceAsync<TResource, TId>(TId id, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnBeforeDeleteResourceAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task OnAfterDeleteResourceAsync<TResource, TId>(TId id, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            dynamic resourceDefinition = ResolveResourceDefinition(typeof(TResource));
            await resourceDefinition.OnAfterDeleteResourceAsync(id, cancellationToken);
        }

        protected virtual object ResolveResourceDefinition(Type resourceType)
        {
            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            if (resourceContext.IdentityType == typeof(int))
            {
                Type intResourceDefinitionType = typeof(IResourceDefinition<>).MakeGenericType(resourceContext.ResourceType);
                object intResourceDefinition = _serviceProvider.GetService(intResourceDefinitionType);

                if (intResourceDefinition != null)
                {
                    return intResourceDefinition;
                }
            }

            Type resourceDefinitionType = typeof(IResourceDefinition<,>).MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }
    }
}
