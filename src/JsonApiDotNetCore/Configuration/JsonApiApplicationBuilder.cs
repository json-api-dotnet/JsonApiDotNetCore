using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.AtomicOperations.Processors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Request;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using JsonApiDotNetCore.Serialization.Response;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// A utility class that builds a JsonApi application. It registers all required services and allows the user to override parts of the startup
    /// configuration.
    /// </summary>
    internal sealed class JsonApiApplicationBuilder : IJsonApiApplicationBuilder, IDisposable
    {
        private readonly JsonApiOptions _options = new();
        private readonly IServiceCollection _services;
        private readonly IMvcCoreBuilder _mvcBuilder;
        private readonly ResourceGraphBuilder _resourceGraphBuilder;
        private readonly ServiceDiscoveryFacade _serviceDiscoveryFacade;
        private readonly ServiceProvider _intermediateProvider;

        public Action<MvcOptions> ConfigureMvcOptions { get; set; }

        public JsonApiApplicationBuilder(IServiceCollection services, IMvcCoreBuilder mvcBuilder)
        {
            ArgumentGuard.NotNull(services, nameof(services));
            ArgumentGuard.NotNull(mvcBuilder, nameof(mvcBuilder));

            _services = services;
            _mvcBuilder = mvcBuilder;
            _intermediateProvider = services.BuildServiceProvider();

            var loggerFactory = _intermediateProvider.GetRequiredService<ILoggerFactory>();

            _resourceGraphBuilder = new ResourceGraphBuilder(_options, loggerFactory);
            _serviceDiscoveryFacade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, loggerFactory);
        }

        /// <summary>
        /// Executes the action provided by the user to configure <see cref="JsonApiOptions" />.
        /// </summary>
        public void ConfigureJsonApiOptions(Action<JsonApiOptions> configureOptions)
        {
            configureOptions?.Invoke(_options);
        }

        /// <summary>
        /// Executes the action provided by the user to configure <see cref="ServiceDiscoveryFacade" />.
        /// </summary>
        public void ConfigureAutoDiscovery(Action<ServiceDiscoveryFacade> configureAutoDiscovery)
        {
            configureAutoDiscovery?.Invoke(_serviceDiscoveryFacade);
        }

        /// <summary>
        /// Configures and builds the resource graph with resources from the provided sources and adds it to the DI container.
        /// </summary>
        public void AddResourceGraph(ICollection<Type> dbContextTypes, Action<ResourceGraphBuilder> configureResourceGraph)
        {
            _serviceDiscoveryFacade.DiscoverResources();

            foreach (Type dbContextType in dbContextTypes)
            {
                var dbContext = (DbContext)_intermediateProvider.GetRequiredService(dbContextType);
                _resourceGraphBuilder.Add(dbContext);
            }

            configureResourceGraph?.Invoke(_resourceGraphBuilder);

            IResourceGraph resourceGraph = _resourceGraphBuilder.Build();

            _options.SerializerOptions.Converters.Add(new ResourceObjectConverter(resourceGraph));

            _services.AddSingleton(resourceGraph);
        }

        /// <summary>
        /// Configures built-in ASP.NET Core MVC components. Most of this configuration can be adjusted for the developers' need.
        /// </summary>
        public void ConfigureMvc()
        {
            _mvcBuilder.AddMvcOptions(options =>
            {
                options.EnableEndpointRouting = true;
                options.Filters.AddService<IAsyncJsonApiExceptionFilter>();
                options.Filters.AddService<IAsyncQueryStringActionFilter>();
                options.Filters.AddService<IAsyncConvertEmptyActionResultFilter>();
                ConfigureMvcOptions?.Invoke(options);
            });

            if (_options.ValidateModelState)
            {
                _mvcBuilder.AddDataAnnotations();
                _services.AddSingleton<IModelMetadataProvider, JsonApiModelMetadataProvider>();
            }
        }

        /// <summary>
        /// Discovers DI registrable services in the assemblies marked for discovery.
        /// </summary>
        public void DiscoverInjectables()
        {
            _serviceDiscoveryFacade.DiscoverInjectables();
        }

        /// <summary>
        /// Registers the remaining internals.
        /// </summary>
        public void ConfigureServiceContainer(ICollection<Type> dbContextTypes)
        {
            if (dbContextTypes.Any())
            {
                _services.AddScoped(typeof(DbContextResolver<>));

                foreach (Type dbContextType in dbContextTypes)
                {
                    Type dbContextResolverType = typeof(DbContextResolver<>).MakeGenericType(dbContextType);
                    _services.AddScoped(typeof(IDbContextResolver), dbContextResolverType);
                }

                _services.AddScoped<IOperationsTransactionFactory, EntityFrameworkCoreTransactionFactory>();
            }
            else
            {
                _services.AddScoped<IOperationsTransactionFactory, MissingTransactionFactory>();
            }

            AddResourceLayer();
            AddRepositoryLayer();
            AddServiceLayer();
            AddMiddlewareLayer();
            AddSerializationLayer();
            AddQueryStringLayer();
            AddOperationsLayer();

            _services.AddScoped(typeof(IResourceChangeTracker<>), typeof(ResourceChangeTracker<>));
            _services.AddScoped<IPaginationContext, PaginationContext>();
            _services.AddScoped<IEvaluatedIncludeCache, EvaluatedIncludeCache>();
            _services.AddScoped<ISparseFieldSetCache, SparseFieldSetCache>();
            _services.AddScoped<IQueryLayerComposer, QueryLayerComposer>();
            _services.AddScoped<IInverseNavigationResolver, InverseNavigationResolver>();
        }

        private void AddMiddlewareLayer()
        {
            _services.AddSingleton<IJsonApiOptions>(_options);
            _services.AddSingleton<IJsonApiApplicationBuilder>(this);
            _services.AddSingleton<IExceptionHandler, ExceptionHandler>();
            _services.AddScoped<IAsyncJsonApiExceptionFilter, AsyncJsonApiExceptionFilter>();
            _services.AddScoped<IAsyncQueryStringActionFilter, AsyncQueryStringActionFilter>();
            _services.AddScoped<IAsyncConvertEmptyActionResultFilter, AsyncConvertEmptyActionResultFilter>();
            _services.AddSingleton<IJsonApiInputFormatter, JsonApiInputFormatter>();
            _services.AddSingleton<IJsonApiOutputFormatter, JsonApiOutputFormatter>();
            _services.AddSingleton<IJsonApiRoutingConvention, JsonApiRoutingConvention>();
            _services.AddSingleton<IControllerResourceMapping>(sp => sp.GetRequiredService<IJsonApiRoutingConvention>());
            _services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddScoped<IJsonApiRequest, JsonApiRequest>();
            _services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            _services.AddScoped<IJsonApiReader, JsonApiReader>();
            _services.AddScoped<ITargetedFields, TargetedFields>();
        }

        private void AddResourceLayer()
        {
            RegisterImplementationForOpenInterfaces(ServiceDiscoveryFacade.ResourceDefinitionInterfaces, typeof(JsonApiResourceDefinition<>),
                typeof(JsonApiResourceDefinition<,>));

            _services.AddScoped<IResourceDefinitionAccessor, ResourceDefinitionAccessor>();
            _services.AddScoped<IResourceFactory, ResourceFactory>();
        }

        private void AddRepositoryLayer()
        {
            RegisterImplementationForOpenInterfaces(ServiceDiscoveryFacade.RepositoryInterfaces, typeof(EntityFrameworkCoreRepository<>),
                typeof(EntityFrameworkCoreRepository<,>));

            _services.AddScoped<IResourceRepositoryAccessor, ResourceRepositoryAccessor>();
        }

        private void AddServiceLayer()
        {
            RegisterImplementationForOpenInterfaces(ServiceDiscoveryFacade.ServiceInterfaces, typeof(JsonApiResourceService<>),
                typeof(JsonApiResourceService<,>));
        }

        private void RegisterImplementationForOpenInterfaces(HashSet<Type> openGenericInterfaces, Type intImplementation, Type implementation)
        {
            foreach (Type openGenericInterface in openGenericInterfaces)
            {
                Type implementationType = openGenericInterface.GetGenericArguments().Length == 1 ? intImplementation : implementation;

                _services.TryAddScoped(openGenericInterface, implementationType);
            }
        }

        private void AddQueryStringLayer()
        {
            _services.AddScoped<IIncludeQueryStringParameterReader, IncludeQueryStringParameterReader>();
            _services.AddScoped<IFilterQueryStringParameterReader, FilterQueryStringParameterReader>();
            _services.AddScoped<ISortQueryStringParameterReader, SortQueryStringParameterReader>();
            _services.AddScoped<ISparseFieldSetQueryStringParameterReader, SparseFieldSetQueryStringParameterReader>();
            _services.AddScoped<IPaginationQueryStringParameterReader, PaginationQueryStringParameterReader>();
            _services.AddScoped<IResourceDefinitionQueryableParameterReader, ResourceDefinitionQueryableParameterReader>();

            RegisterDependentService<IQueryStringParameterReader, IIncludeQueryStringParameterReader>();
            RegisterDependentService<IQueryStringParameterReader, IFilterQueryStringParameterReader>();
            RegisterDependentService<IQueryStringParameterReader, ISortQueryStringParameterReader>();
            RegisterDependentService<IQueryStringParameterReader, ISparseFieldSetQueryStringParameterReader>();
            RegisterDependentService<IQueryStringParameterReader, IPaginationQueryStringParameterReader>();
            RegisterDependentService<IQueryStringParameterReader, IResourceDefinitionQueryableParameterReader>();

            RegisterDependentService<IQueryConstraintProvider, IIncludeQueryStringParameterReader>();
            RegisterDependentService<IQueryConstraintProvider, IFilterQueryStringParameterReader>();
            RegisterDependentService<IQueryConstraintProvider, ISortQueryStringParameterReader>();
            RegisterDependentService<IQueryConstraintProvider, ISparseFieldSetQueryStringParameterReader>();
            RegisterDependentService<IQueryConstraintProvider, IPaginationQueryStringParameterReader>();
            RegisterDependentService<IQueryConstraintProvider, IResourceDefinitionQueryableParameterReader>();

            _services.AddScoped<IQueryStringReader, QueryStringReader>();
            _services.AddSingleton<IRequestQueryStringAccessor, RequestQueryStringAccessor>();
        }

        private void RegisterDependentService<TCollectionElement, TElementToAdd>()
            where TCollectionElement : class
            where TElementToAdd : TCollectionElement
        {
            _services.AddScoped<TCollectionElement>(serviceProvider => serviceProvider.GetRequiredService<TElementToAdd>());
        }

        private void AddSerializationLayer()
        {
            _services.AddScoped<IResourceIdentifierObjectAdapter, ResourceIdentifierObjectAdapter>();
            _services.AddScoped<IRelationshipDataAdapter, RelationshipDataAdapter>();
            _services.AddScoped<IResourceObjectAdapter, ResourceObjectAdapter>();
            _services.AddScoped<IResourceDataAdapter, ResourceDataAdapter>();
            _services.AddScoped<IAtomicReferenceAdapter, AtomicReferenceAdapter>();
            _services.AddScoped<IResourceDataInOperationsRequestAdapter, ResourceDataInOperationsRequestAdapter>();
            _services.AddScoped<IAtomicOperationObjectAdapter, AtomicOperationObjectAdapter>();
            _services.AddScoped<IDocumentInResourceOrRelationshipRequestAdapter, DocumentInResourceOrRelationshipRequestAdapter>();
            _services.AddScoped<IDocumentInOperationsRequestAdapter, DocumentInOperationsRequestAdapter>();
            _services.AddScoped<IDocumentAdapter, DocumentAdapter>();

            _services.AddScoped<ILinkBuilder, LinkBuilder>();
            _services.AddScoped<IResponseMeta, EmptyResponseMeta>();
            _services.AddScoped<IMetaBuilder, MetaBuilder>();
            _services.AddSingleton<IFingerprintGenerator, FingerprintGenerator>();
            _services.AddSingleton<IETagGenerator, ETagGenerator>();
            _services.AddScoped<IResponseModelAdapter, ResponseModelAdapter>();
        }

        private void AddOperationsLayer()
        {
            _services.AddScoped(typeof(ICreateProcessor<,>), typeof(CreateProcessor<,>));
            _services.AddScoped(typeof(IUpdateProcessor<,>), typeof(UpdateProcessor<,>));
            _services.AddScoped(typeof(IDeleteProcessor<,>), typeof(DeleteProcessor<,>));
            _services.AddScoped(typeof(IAddToRelationshipProcessor<,>), typeof(AddToRelationshipProcessor<,>));
            _services.AddScoped(typeof(ISetRelationshipProcessor<,>), typeof(SetRelationshipProcessor<,>));
            _services.AddScoped(typeof(IRemoveFromRelationshipProcessor<,>), typeof(RemoveFromRelationshipProcessor<,>));

            _services.AddScoped<IOperationsProcessor, OperationsProcessor>();
            _services.AddScoped<IOperationProcessorAccessor, OperationProcessorAccessor>();
            _services.AddScoped<ILocalIdTracker, LocalIdTracker>();
        }

        public void Dispose()
        {
            _intermediateProvider.Dispose();
        }
    }
}
