using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Serialization.Server;
using JsonApiDotNetCore.Serialization.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;
using JsonApiDotNetCore.Builders;

namespace JsonApiDotNetCore.Builders
{
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


        public void ConfigureJsonApiOptions(Action<JsonApiOptions> configureOptions) => configureOptions(JsonApiOptions);

        public void ConfigureMvc()
        {
            RegisterJsonApiStartupServices();

            var intermediateProvider = _services.BuildServiceProvider();
            _resourceGraphBuilder = intermediateProvider.GetRequiredService<IResourceGraphBuilder>();
            _serviceDiscoveryFacade = intermediateProvider.GetRequiredService<IServiceDiscoveryFacade>();
            var exceptionFilterProvider = intermediateProvider.GetRequiredService<IJsonApiExceptionFilterProvider>();
            var typeMatchFilterProvider = intermediateProvider.GetRequiredService<IJsonApiTypeMatchFilterProvider>();

            _mvcBuilder.AddMvcOptions(mvcOptions =>
            {
                mvcOptions.Filters.Add(exceptionFilterProvider.Get());
                mvcOptions.Filters.Add(typeMatchFilterProvider.Get());
                mvcOptions.InputFormatters.Insert(0, new JsonApiInputFormatter());
                mvcOptions.OutputFormatters.Insert(0, new JsonApiOutputFormatter());
            });

            var routingConvention = intermediateProvider.GetRequiredService<IJsonApiRoutingConvention>();
            _mvcBuilder.AddMvcOptions(opt => opt.Conventions.Insert(0, routingConvention));
            _services.AddSingleton(routingConvention); // <--- why is this needed?
        }

        public void AutoDiscover(Action<IServiceDiscoveryFacade> autoDiscover)
        {
            autoDiscover(_serviceDiscoveryFacade);
        }

        public void ConfigureResources(Action<IResourceGraphBuilder> resourceGraphBuilder)
        {
            resourceGraphBuilder(_resourceGraphBuilder);
        }

        public void ConfigureResources<TContext>(Action<IResourceGraphBuilder> resourceGraphBuilder) where TContext : DbContext
        {
            _resourceGraphBuilder.AddDbContext<TContext>();
            _usesDbContext = true;
            _services.AddScoped<IDbContextResolver, DbContextResolver<TContext>>();
            resourceGraphBuilder?.Invoke(_resourceGraphBuilder);
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

        public void ConfigureServices()
        {
            var graph = _resourceGraphBuilder.Build();

            if (!_usesDbContext)
            {
                _services.AddScoped<DbContext>();
                _services.AddSingleton(new DbContextOptionsBuilder().Options);
            }

            _services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            _services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));

            _services.AddScoped(typeof(IEntityReadRepository<,>), typeof(DefaultEntityRepository<,>));
            _services.AddScoped(typeof(IEntityWriteRepository<,>), typeof(DefaultEntityRepository<,>));

            _services.AddScoped(typeof(ICreateService<>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(ICreateService<,>), typeof(EntityResourceService<,>));

            _services.AddScoped(typeof(IGetAllService<>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(IGetAllService<,>), typeof(EntityResourceService<,>));

            _services.AddScoped(typeof(IGetByIdService<>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(IGetByIdService<,>), typeof(EntityResourceService<,>));

            _services.AddScoped(typeof(IGetRelationshipService<,>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(IGetRelationshipService<,>), typeof(EntityResourceService<,>));

            _services.AddScoped(typeof(IUpdateService<>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(IUpdateService<,>), typeof(EntityResourceService<,>));

            _services.AddScoped(typeof(IDeleteService<>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(IDeleteService<,>), typeof(EntityResourceService<,>));

            _services.AddScoped(typeof(IResourceService<>), typeof(EntityResourceService<>));
            _services.AddScoped(typeof(IResourceService<,>), typeof(EntityResourceService<,>));

            _services.AddSingleton<ILinksConfiguration>(JsonApiOptions);
            _services.AddSingleton(graph);
            _services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddSingleton<IContextEntityProvider>(graph);
            _services.AddScoped<ICurrentRequest, CurrentRequest>();
            _services.AddScoped<IScopedServiceProvider, RequestScopedServiceProvider>();
            _services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            _services.AddScoped<IJsonApiReader, JsonApiReader>();
            _services.AddScoped<IGenericProcessorFactory, GenericProcessorFactory>();
            _services.AddScoped(typeof(GenericProcessor<>));
            _services.AddScoped<IQueryParameterDiscovery, QueryParameterDiscovery>();
            _services.AddScoped<ITargetedFields, TargetedFields>();
            _services.AddScoped<IFieldsExplorer, FieldsExplorer>();
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
    }
}
