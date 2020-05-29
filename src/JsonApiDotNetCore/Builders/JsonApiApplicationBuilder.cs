using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.QueryStrings;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Serialization.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.RequestServices.Contracts;

namespace JsonApiDotNetCore.Builders
{
    /// <summary>
    /// A utility class that builds a JsonApi application. It registers all required services
    /// and allows the user to override parts of the startup configuration.
    /// </summary>
    internal sealed class JsonApiApplicationBuilder
    {
        private readonly JsonApiOptions _options = new JsonApiOptions();
        private IResourceGraphBuilder _resourceGraphBuilder;
        private Type _dbContextType;
        private readonly IServiceCollection _services;
        private IServiceDiscoveryFacade _serviceDiscoveryFacade;
        private readonly IMvcCoreBuilder _mvcBuilder;

        public JsonApiApplicationBuilder(IServiceCollection services, IMvcCoreBuilder mvcBuilder)
        {
            _services = services;
            _mvcBuilder = mvcBuilder;
        }

        /// <summary>
        /// Executes the action provided by the user to configure <see cref="JsonApiOptions"/>
        /// </summary>
        public void ConfigureJsonApiOptions(Action<JsonApiOptions> options)
        {
            options?.Invoke(_options);
        }

        /// <summary>
        /// Configures built-in .NET Core MVC (things like middleware, routing). Most of this configuration can be adjusted for the developers' need.
        /// Before calling .AddJsonApi(), a developer can register their own implementation of the following services to customize startup:
        /// <see cref="IResourceGraphBuilder"/>, <see cref="IServiceDiscoveryFacade"/>, <see cref="IJsonApiExceptionFilterProvider"/>,
        /// <see cref="IJsonApiTypeMatchFilterProvider"/> and <see cref="IJsonApiRoutingConvention"/>.
        /// </summary>
        public void ConfigureMvc(Type dbContextType)
        {
            RegisterJsonApiStartupServices();

            IJsonApiExceptionFilterProvider exceptionFilterProvider;
            IJsonApiTypeMatchFilterProvider typeMatchFilterProvider;
            IJsonApiRoutingConvention routingConvention;

            using (var intermediateProvider = _services.BuildServiceProvider())
            {
                _resourceGraphBuilder = intermediateProvider.GetRequiredService<IResourceGraphBuilder>();
                _serviceDiscoveryFacade = intermediateProvider.GetRequiredService<IServiceDiscoveryFacade>();
                _dbContextType = dbContextType;

                AddResourceTypesFromDbContext(intermediateProvider);

                exceptionFilterProvider = intermediateProvider.GetRequiredService<IJsonApiExceptionFilterProvider>();
                typeMatchFilterProvider = intermediateProvider.GetRequiredService<IJsonApiTypeMatchFilterProvider>();
                routingConvention = intermediateProvider.GetRequiredService<IJsonApiRoutingConvention>();
            }

            _mvcBuilder.AddMvcOptions(options =>
            {
                options.EnableEndpointRouting = true;
                options.Filters.Add(exceptionFilterProvider.Get());
                options.Filters.Add(typeMatchFilterProvider.Get());
                options.Filters.Add(new ConvertEmptyActionResultFilter());
                options.InputFormatters.Insert(0, new JsonApiInputFormatter());
                options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());
                options.Conventions.Insert(0, routingConvention);
            });

            if (_options.ValidateModelState)
            {
                _mvcBuilder.AddDataAnnotations();
            }

            _services.AddSingleton<IControllerResourceMapping>(routingConvention);
        }

        private void AddResourceTypesFromDbContext(ServiceProvider intermediateProvider)
        {
            if (_dbContextType != null)
            {
                var dbContext = (DbContext) intermediateProvider.GetRequiredService(_dbContextType);

                foreach (var entityType in dbContext.Model.GetEntityTypes())
                {
                    _resourceGraphBuilder.AddResource(entityType.ClrType);
                }
            }
        }

        /// <summary>
        /// Executes auto-discovery of JADNC services.
        /// </summary>
        public void AutoDiscover(Action<IServiceDiscoveryFacade> autoDiscover)
        {
            autoDiscover?.Invoke(_serviceDiscoveryFacade);
        }

        /// <summary>
        /// Executes the action provided by the user to configure the resources using <see cref="IResourceGraphBuilder"/>
        /// </summary>
        public void ConfigureResources(Action<IResourceGraphBuilder> resources)
        {
            resources?.Invoke(_resourceGraphBuilder);
        }

        /// <summary>
        /// Registers the remaining internals.
        /// </summary>
        public void ConfigureServices()
        {
            var resourceGraph = _resourceGraphBuilder.Build();

            if (_dbContextType != null)
            {
                var contextResolverType = typeof(DbContextResolver<>).MakeGenericType(_dbContextType);
                _services.AddScoped(typeof(IDbContextResolver), contextResolverType);
            }
            else
            {
                _services.AddScoped<DbContext>();
                _services.AddSingleton(new DbContextOptionsBuilder().Options);
            }

            _services.AddScoped(typeof(IResourceRepository<>), typeof(EntityFrameworkCoreRepository<>));
            _services.AddScoped(typeof(IResourceRepository<,>), typeof(EntityFrameworkCoreRepository<,>));

            _services.AddScoped(typeof(IResourceReadRepository<,>), typeof(EntityFrameworkCoreRepository<,>));
            _services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(EntityFrameworkCoreRepository<,>));

            _services.AddScoped(typeof(ICreateService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(ICreateService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IGetAllService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IGetAllService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IGetByIdService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IGetByIdService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IGetRelationshipService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IGetRelationshipService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IGetSecondaryService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IGetSecondaryService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IUpdateService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IUpdateService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IDeleteService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IDeleteService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IResourceService<>), typeof(JsonApiResourceService<>));
            _services.AddScoped(typeof(IResourceService<,>), typeof(JsonApiResourceService<,>));

            _services.AddScoped(typeof(IResourceQueryService<,>), typeof(JsonApiResourceService<,>));
            _services.AddScoped(typeof(IResourceCommandService<,>), typeof(JsonApiResourceService<,>));

            _services.AddSingleton(resourceGraph);
            _services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddSingleton<IResourceContextProvider>(resourceGraph);
            _services.AddSingleton<IExceptionHandler, ExceptionHandler>();

            _services.AddScoped<ICurrentRequest, CurrentRequest>();
            _services.AddScoped<IScopedServiceProvider, RequestScopedServiceProvider>();
            _services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            _services.AddScoped<IJsonApiReader, JsonApiReader>();
            _services.AddScoped<IGenericServiceFactory, GenericServiceFactory>();
            _services.AddScoped(typeof(RepositoryRelationshipUpdateHelper<>));
            _services.AddScoped<ITargetedFields, TargetedFields>();
            _services.AddScoped<IResourceDefinitionProvider, ResourceDefinitionProvider>();
            _services.AddScoped<IFieldsToSerialize, FieldsToSerialize>();
            _services.AddScoped(typeof(IResourceChangeTracker<>), typeof(ResourceChangeTracker<>));
            _services.AddScoped<IResourceFactory, ResourceFactory>();
            _services.AddScoped<IPaginationContext, PaginationContext>();
            _services.AddScoped<IQueryLayerComposer, QueryLayerComposer>();

            AddServerSerialization();
            AddQueryStringParameterServices();
            if (_options.EnableResourceHooks)
                AddResourceHooks();

            _services.AddScoped<IInverseRelationships, InverseRelationships>();
        }

        private void AddQueryStringParameterServices()
        {
            _services.AddScoped<IIncludeQueryStringParameterReader, IncludeQueryStringParameterReader>();
            _services.AddScoped<IFilterQueryStringParameterReader, FilterQueryStringParameterReader>();
            _services.AddScoped<ISortQueryStringParameterReader, SortQueryStringParameterReader>();
            _services.AddScoped<ISparseFieldSetQueryStringParameterReader, SparseFieldSetQueryStringParameterReader>();
            _services.AddScoped<IPaginationQueryStringParameterReader, PaginationQueryStringParameterReader>();
            _services.AddScoped<IDefaultsQueryStringParameterReader, DefaultsQueryStringParameterReader>();
            _services.AddScoped<INullsQueryStringParameterReader, NullsQueryStringParameterReader>();
            _services.AddScoped<IResourceDefinitionQueryableParameterReader, ResourceDefinitionQueryableParameterReader>();

            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<IIncludeQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<IFilterQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<ISortQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<ISparseFieldSetQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<IPaginationQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<IDefaultsQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<INullsQueryStringParameterReader>());
            _services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<IResourceDefinitionQueryableParameterReader>());

            _services.AddScoped<IQueryConstraintProvider>(sp => sp.GetService<IIncludeQueryStringParameterReader>());
            _services.AddScoped<IQueryConstraintProvider>(sp => sp.GetService<IFilterQueryStringParameterReader>());
            _services.AddScoped<IQueryConstraintProvider>(sp => sp.GetService<ISortQueryStringParameterReader>());
            _services.AddScoped<IQueryConstraintProvider>(sp => sp.GetService<ISparseFieldSetQueryStringParameterReader>());
            _services.AddScoped<IQueryConstraintProvider>(sp => sp.GetService<IPaginationQueryStringParameterReader>());
            _services.AddScoped<IQueryConstraintProvider>(sp => sp.GetService<IResourceDefinitionQueryableParameterReader>());

            _services.AddScoped<IQueryStringActionFilter, QueryStringActionFilter>();
            _services.AddScoped<IQueryStringReader, QueryStringReader>();
            _services.AddSingleton<IRequestQueryStringAccessor, RequestQueryStringAccessor>();
        }

        private void AddResourceHooks()
        {
            _services.AddSingleton(typeof(IHooksDiscovery<>), typeof(HooksDiscovery<>));
            _services.AddScoped(typeof(IResourceHookContainer<>), typeof(ResourceDefinition<>));
            _services.AddTransient(typeof(IResourceHookExecutor), typeof(ResourceHookExecutor));
            _services.AddTransient<IHookExecutorHelper, HookExecutorHelper>();
            _services.AddTransient<ITraversalHelper, TraversalHelper>();
        }

        private void AddServerSerialization()
        {
            _services.AddScoped<IIncludedResourceObjectBuilder, IncludedResourceObjectBuilder>();
            _services.AddScoped<IJsonApiDeserializer, RequestDeserializer>();
            _services.AddScoped<IResourceObjectBuilderSettingsProvider, ResourceObjectBuilderSettingsProvider>();
            _services.AddScoped<IJsonApiSerializerFactory, ResponseSerializerFactory>();
            _services.AddScoped<ILinkBuilder, LinkBuilder>();
            _services.AddScoped(typeof(IMetaBuilder<>), typeof(MetaBuilder<>));
            _services.AddScoped(typeof(ResponseSerializer<>));
            _services.AddScoped(sp => sp.GetRequiredService<IJsonApiSerializerFactory>().GetSerializer());
            _services.AddScoped<IResourceObjectBuilder, ResponseResourceObjectBuilder>();
        }

        private void RegisterJsonApiStartupServices()
        {
            _services.AddSingleton<IJsonApiOptions>(_options);
            _services.TryAddSingleton<IJsonApiRoutingConvention, JsonApiRoutingConvention>();
            _services.TryAddSingleton<IResourceGraphBuilder, ResourceGraphBuilder>();
            _services.TryAddSingleton<IServiceDiscoveryFacade>(sp => new ServiceDiscoveryFacade(_services, sp.GetRequiredService<IResourceGraphBuilder>()));
            _services.TryAddScoped<IJsonApiExceptionFilterProvider, JsonApiExceptionFilterProvider>();
            _services.TryAddScoped<IJsonApiTypeMatchFilterProvider, JsonApiTypeMatchFilterProvider>();
        }
    }
}
