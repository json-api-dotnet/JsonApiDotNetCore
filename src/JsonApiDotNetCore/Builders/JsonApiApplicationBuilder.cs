using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
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

namespace JsonApiDotNetCore.Builders
{
    /// <summary>
    /// A utility class that builds a JsonApi application. It registers all required services
    /// and allows the user to override parts of the startup configuration.
    /// </summary>
    public class JsonApiApplicationBuilder
    {
        public readonly JsonApiOptions JsonApiOptions = new JsonApiOptions();
        private IResourceGraphBuilder _resourceGraphBuilder;
        private IServiceDiscoveryFacade _serviceDiscoveryFacade;
        private bool _usesDbContext;
        private readonly IServiceCollection _services;
        private readonly IMvcCoreBuilder _mvcBuilder;

        public JsonApiApplicationBuilder(IServiceCollection services, IMvcCoreBuilder mvcBuilder)
        {
            _services = services;
            _mvcBuilder = mvcBuilder;
        }

        internal void ConfigureLogging()
        {
            _services.AddLogging();
        }

        /// <summary>
        /// Executes the action provided by the user to configure <see cref="JsonApiOptions"/>
        /// </summary>
        public void ConfigureJsonApiOptions(Action<JsonApiOptions> configureOptions) => configureOptions(JsonApiOptions);

        /// <summary>
        /// Configures built-in .net core MVC (things like middleware, routing). Most of this configuration can be adjusted for the developers need.
        /// Before calling .AddJsonApi(), a developer can register their own implementation of the following services to customize startup:
        /// <see cref="IResourceGraphBuilder"/>, <see cref="IServiceDiscoveryFacade"/>, <see cref="IJsonApiExceptionFilterProvider"/>,
        /// <see cref="IJsonApiTypeMatchFilterProvider"/>, <see cref="IJsonApiRoutingConvention"/> and <see cref="IResourceNameFormatter"/>.
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
                options.InputFormatters.Insert(0, new JsonApiInputFormatter());
                options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());
                options.Conventions.Insert(0, routingConvention);
            });
            _services.AddSingleton<IControllerResourceMapping>(routingConvention);
        }

        /// <summary>
        /// Executes autodiscovery of JADNC services.
        /// </summary>
        public void AutoDiscover(Action<IServiceDiscoveryFacade> autoDiscover)
        {
            autoDiscover(_serviceDiscoveryFacade);
        }

        /// <summary>
        /// Executes the action provided by the user to configure the resources using <see cref="IResourceGraphBuilder"/>
        /// </summary>
        /// <param name="resourceGraphBuilder"></param>
        public void ConfigureResources(Action<IResourceGraphBuilder> resourceGraphBuilder)
        {
            resourceGraphBuilder(_resourceGraphBuilder);
        }

        /// <summary>
        /// Executes the action provided by the user to configure the resources using <see cref="IResourceGraphBuilder"/>.
        /// Additionally, inspects the EF core database context for models that implement IIdentifiable.
        /// </summary>
        public void ConfigureResources<TContext>(Action<IResourceGraphBuilder> resourceGraphBuilder) where TContext : DbContext
        {
            _resourceGraphBuilder.AddDbContext<TContext>();
            _usesDbContext = true;
            _services.AddScoped<IDbContextResolver, DbContextResolver<TContext>>();
            resourceGraphBuilder?.Invoke(_resourceGraphBuilder);
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
            _services.AddScoped(typeof(IResourceCmdService<,>), typeof(DefaultResourceService<,>));

            _services.AddSingleton<ILinksConfiguration>(JsonApiOptions);
            _services.AddSingleton(resourceGraph);
            _services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddSingleton(resourceGraph);
            _services.AddSingleton<IResourceContextProvider>(resourceGraph);
            _services.AddScoped<ICurrentRequest, CurrentRequest>();
            _services.AddScoped<IScopedServiceProvider, RequestScopedServiceProvider>();
            _services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            _services.AddScoped<IJsonApiReader, JsonApiReader>();
            _services.AddScoped<IGenericServiceFactory, GenericServiceFactory>();
            _services.AddScoped(typeof(RepositoryRelationshipUpdateHelper<>));
            _services.AddScoped<IQueryParameterDiscovery, QueryParameterDiscovery>();
            _services.AddScoped<ITargetedFields, TargetedFields>();
            _services.AddScoped<IResourceDefinitionProvider, ResourceDefinitionProvider>();
            _services.AddScoped<IFieldsToSerialize, FieldsToSerialize>();
            _services.AddScoped<IQueryParameterActionFilter, QueryParameterActionFilter>();

            AddServerSerialization();
            AddQueryParameterServices();
            if (JsonApiOptions.EnableResourceHooks)
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
            _services.AddScoped<IOmitDefaultService, OmitDefaultService>();
            _services.AddScoped<IOmitNullService, OmitNullService>();

            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IIncludeService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IFilterService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<ISortService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<ISparseFieldsService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IPageService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IOmitDefaultService>());
            _services.AddScoped<IQueryParameterService>(sp => sp.GetService<IOmitNullService>());
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
            _services.AddSingleton<IJsonApiOptions>(JsonApiOptions);
            _services.TryAddSingleton<IResourceNameFormatter>(new KebabCaseFormatter());
            _services.TryAddSingleton<IJsonApiRoutingConvention, DefaultRoutingConvention>();
            _services.TryAddSingleton<IResourceGraphBuilder, ResourceGraphBuilder>();
            _services.TryAddSingleton<IServiceDiscoveryFacade>(sp => new ServiceDiscoveryFacade(_services, sp.GetRequiredService<IResourceGraphBuilder>()));
            _services.TryAddScoped<IJsonApiExceptionFilterProvider, JsonApiExceptionFilterProvider>();
            _services.TryAddScoped<IJsonApiTypeMatchFilterProvider, JsonApiTypeMatchFilterProvider>();
        }
    }
}
