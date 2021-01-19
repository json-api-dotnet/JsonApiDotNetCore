using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    public class ResourceDefinitionAccessor : IResourceDefinitionAccessor
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IServiceProvider _serviceProvider;

        public ResourceDefinitionAccessor(IResourceContextProvider resourceContextProvider, IServiceProvider serviceProvider)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(Type resourceType, IReadOnlyCollection<IncludeElementExpression> existingIncludes)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyIncludes(existingIncludes);
        }

        /// <inheritdoc />
        public FilterExpression OnApplyFilter(Type resourceType, FilterExpression existingFilter)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyFilter(existingFilter);
        }

        /// <inheritdoc />
        public SortExpression OnApplySort(Type resourceType, SortExpression existingSort)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplySort(existingSort);
        }

        /// <inheritdoc />
        public PaginationExpression OnApplyPagination(Type resourceType, PaginationExpression existingPagination)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplyPagination(existingPagination);
        }

        /// <inheritdoc />
        public SparseFieldSetExpression OnApplySparseFieldSet(Type resourceType, SparseFieldSetExpression existingSparseFieldSet)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.OnApplySparseFieldSet(existingSparseFieldSet);
        }

        /// <inheritdoc />
        public object GetQueryableHandlerForQueryStringParameter(Type resourceType, string parameterName)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            var handlers = resourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters();

            return handlers != null && handlers.ContainsKey(parameterName) ? handlers[parameterName] : null;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetMeta(Type resourceType, IIdentifiable resourceInstance)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            dynamic resourceDefinition = ResolveResourceDefinition(resourceType);
            return resourceDefinition.GetMeta((dynamic) resourceInstance);
        }

        protected virtual object ResolveResourceDefinition(Type resourceType)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            if (resourceContext.IdentityType == typeof(int))
            {
                var intResourceDefinitionType = typeof(IResourceDefinition<>).MakeGenericType(resourceContext.ResourceType);
                var intResourceDefinition = _serviceProvider.GetService(intResourceDefinitionType);
                
                if (intResourceDefinition != null)
                {
                    return intResourceDefinition;
                }
            }

            var resourceDefinitionType = typeof(IResourceDefinition<,>).MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }
    }
}
