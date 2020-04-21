using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions.EntityFrameworkCore;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Serialization.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;
using JsonApiDotNetCore.QueryParameterServices.Common;
using JsonApiDotNetCore.RequestServices;

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
        private bool _usesDbContext;
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
        public void ConfigureMvc()
        {
            RegisterJsonApiStartupServices();

            var intermediateProvider = _services.BuildServiceProvider();
            _resourceGraphBuilder = intermediateProvider.GetRequiredService<IResourceGraphBuilder>();
            _serviceDiscoveryFacade = intermediateProvider.GetRequiredService<IServiceDiscoveryFacade>();
            var exceptionFilterProvider = intermediateProvider.GetRequiredService<IJsonApiExceptionFilterProvider>();
            var typeMatchFilterProvider = intermediateProvider.GetRequiredService<IJsonApiTypeMatchFilterProvider>();
            var routingConvention = intermediateProvider.GetRequiredService<IJsonApiRoutingConvention>();

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

        /// <summary>
        /// Executes auto-discovery of JADNC services.
        /// </summary>
        public void AutoDiscover(Action<IServiceDiscoveryFacade> autoDiscover)
        {
            autoDiscover?.Invoke(_serviceDiscoveryFacade);
        }

        public void ConfigureResourcesFromDbContext<TDbContext>(Action<IResourceGraphBuilder> resources)
            where TDbContext : DbContext
        {
            _resourceGraphBuilder.AddDbContext<TDbContext>();
            _usesDbContext = true;
            _services.AddScoped<IDbContextResolver, DbContextResolver<TDbContext>>();

            ConfigureResources(resources);
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

            if (!_usesDbContext)
            {
                _services.AddScoped<DbContext>();
                _services.AddSingleton(new DbContextOptionsBuilder().Options);
            }

            _services.AddScoped(typeof(IResourceRepository<>), typeof(DefaultResourceRepository<>));
            _services.AddScoped(typeof(IResourceRepository<,>), typeof(DefaultResourceRepository<,>));

            _services.AddScoped(typeof(IResourceReadRepository<,>), typeof(DefaultResourceRepository<,>));
            _services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(DefaultResourceRepository<,>));

            _services.AddScoped(typeof(ICreateService<>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(ICreateService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IGetAllService<>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(IGetAllService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IGetByIdService<>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(IGetByIdService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IGetRelationshipService<,>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(IGetRelationshipService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IUpdateService<>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(IUpdateService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IDeleteService<>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(IDeleteService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IResourceService<>), typeof(DefaultResourceService<>));
            _services.AddScoped(typeof(IResourceService<,>), typeof(DefaultResourceService<,>));

            _services.AddScoped(typeof(IResourceQueryService<,>), typeof(DefaultResourceService<,>));
            _services.AddScoped(typeof(IResourceCommandService<,>), typeof(DefaultResourceService<,>));

            _services.AddSingleton<ILinksConfiguration>(_options);
            _services.AddSingleton(resourceGraph);
            _services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddSingleton<IResourceContextProvider>(resourceGraph);
            _services.AddSingleton<IRequestQueryStringAccessor, RequestQueryStringAccessor>();
            _services.AddSingleton<IExceptionHandler, DefaultExceptionHandler>();

            _services.AddScoped<ICurrentRequest, CurrentRequest>();
            _services.AddScoped<IScopedServiceProvider, RequestScopedServiceProvider>();
            _services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            _services.AddScoped<IJsonApiReader, JsonApiReader>();
            _services.AddScoped<IGenericServiceFactory, GenericServiceFactory>();
            _services.AddScoped(typeof(RepositoryRelationshipUpdateHelper<>));
            _services.AddScoped<IQueryParameterParser, QueryParameterParser>();
            _services.AddScoped<ITargetedFields, TargetedFields>();
            _services.AddScoped<IResourceDefinitionProvider, ResourceDefinitionProvider>();
            _services.AddScoped<IFieldsToSerialize, FieldsToSerialize>();
            _services.AddScoped(typeof(IResourceChangeTracker<>), typeof(DefaultResourceChangeTracker<>));
            _services.AddScoped<IQueryParameterActionFilter, QueryParameterActionFilter>();

            AddServerSerialization();
            AddQueryParameterServices();
            if (_options.EnableResourceHooks)
                AddResourceHooks();

            _services.AddScoped<IInverseRelationships, InverseRelationships>();
        }

        private void AddQueryParameterServices()
        {
            _services.AddScoped<IIncludeService, IncludeService>();
            _services.AddScoped<IFilterService, FilterService>();
            _services.AddScoped<ISortService, SortService>();
            _services.AddScoped<ISparseFieldsService, SparseFieldsService>();
            _services.AddScoped<IPageService, PageService>();
            _services.AddScoped<IDefaultsService, DefaultsService>();
            _services.AddScoped<INullsService, NullsService>();

            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IIncludeService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IFilterService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<ISortService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<ISparseFieldsService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IPageService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IDefaultsService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<INullsService>());
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
            _services.TryAddSingleton<IJsonApiRoutingConvention, DefaultRoutingConvention>();
            _services.TryAddSingleton<IResourceGraphBuilder, ResourceGraphBuilder>();
            _services.TryAddSingleton<IServiceDiscoveryFacade>(sp => new ServiceDiscoveryFacade(_services, sp.GetRequiredService<IResourceGraphBuilder>()));
            _services.TryAddScoped<IJsonApiExceptionFilterProvider, JsonApiExceptionFilterProvider>();
            _services.TryAddScoped<IJsonApiTypeMatchFilterProvider, JsonApiTypeMatchFilterProvider>();
        }
    }
}
