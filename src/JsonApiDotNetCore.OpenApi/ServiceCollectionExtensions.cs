using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the OpenAPI integration to JsonApiDotNetCore by configuring Swashbuckle.
        /// </summary>
        public static void AddOpenApi(this IServiceCollection services, IMvcCoreBuilder mvcBuilder, Action<SwaggerGenOptions> setupSwaggerGenAction = null)
        {
            ArgumentGuard.NotNull(services, nameof(services));
            ArgumentGuard.NotNull(mvcBuilder, nameof(mvcBuilder));

            AddCustomApiExplorer(services, mvcBuilder);

            AddCustomSwaggerComponents(services);

            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();
            AddSwaggerGenerator(scope, services, setupSwaggerGenAction);
            AddSwashbuckleCliCompatibility(scope, mvcBuilder);
            AddOpenApiEndpointConvention(scope, mvcBuilder);
        }

        private static void AddCustomApiExplorer(IServiceCollection services, IMvcCoreBuilder mvcBuilder)
        {
            services.AddSingleton<IApiDescriptionGroupCollectionProvider>(provider =>
            {
                var resourceGraph = provider.GetRequiredService<IResourceGraph>();
                var controllerResourceMapping = provider.GetRequiredService<IControllerResourceMapping>();
                var actionDescriptorCollectionProvider = provider.GetRequiredService<IActionDescriptorCollectionProvider>();
                var apiDescriptionProviders = provider.GetRequiredService<IEnumerable<IApiDescriptionProvider>>();

                JsonApiActionDescriptorCollectionProvider descriptorCollectionProviderWrapper =
                    new(resourceGraph, controllerResourceMapping, actionDescriptorCollectionProvider);

                return new ApiDescriptionGroupCollectionProvider(descriptorCollectionProviderWrapper, apiDescriptionProviders);
            });

            mvcBuilder.AddApiExplorer();

            mvcBuilder.AddMvcOptions(options => options.InputFormatters.Add(new JsonApiRequestFormatMetadataProvider()));
        }

        private static void AddSwaggerGenerator(IServiceScope scope, IServiceCollection services, Action<SwaggerGenOptions> setupSwaggerGenAction)
        {
            var controllerResourceMapping = scope.ServiceProvider.GetRequiredService<IControllerResourceMapping>();
            var resourceGraph = scope.ServiceProvider.GetRequiredService<IResourceGraph>();
            var jsonApiOptions = scope.ServiceProvider.GetRequiredService<IJsonApiOptions>();
            JsonNamingPolicy namingPolicy = jsonApiOptions.SerializerOptions.PropertyNamingPolicy;

            AddSchemaGenerator(services);

            services.AddSwaggerGen(swaggerGenOptions =>
            {
                SetOperationInfo(swaggerGenOptions, controllerResourceMapping, resourceGraph, namingPolicy);
                SetSchemaIdSelector(swaggerGenOptions, resourceGraph, namingPolicy);
                swaggerGenOptions.DocumentFilter<EndpointOrderingFilter>();

                setupSwaggerGenAction?.Invoke(swaggerGenOptions);
            });
        }

        private static void AddSchemaGenerator(IServiceCollection services)
        {
            services.AddSingleton<SchemaGenerator>();
            services.AddSingleton<ISchemaGenerator, JsonApiSchemaGenerator>();
        }

        private static void SetOperationInfo(SwaggerGenOptions swaggerGenOptions, IControllerResourceMapping controllerResourceMapping,
            IResourceGraph resourceGraph, JsonNamingPolicy namingPolicy)
        {
            swaggerGenOptions.TagActionsBy(description => GetOperationTags(description, controllerResourceMapping, resourceGraph));

            JsonApiOperationIdSelector jsonApiOperationIdSelector = new(controllerResourceMapping, namingPolicy);
            swaggerGenOptions.CustomOperationIds(jsonApiOperationIdSelector.GetOperationId);
        }

        private static IList<string> GetOperationTags(ApiDescription description, IControllerResourceMapping controllerResourceMapping,
            IResourceGraph resourceGraph)
        {
            MethodInfo actionMethod = description.ActionDescriptor.GetActionMethod();
            Type resourceType = controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);
            ResourceContext resourceContext = resourceGraph.GetResourceContext(resourceType);

            return new[]
            {
                resourceContext.PublicName
            };
        }

        private static void SetSchemaIdSelector(SwaggerGenOptions swaggerGenOptions, IResourceGraph resourceGraph, JsonNamingPolicy namingPolicy)
        {
            JsonApiSchemaIdSelector jsonApiObjectSchemaSelector = new(namingPolicy, resourceGraph);

            swaggerGenOptions.CustomSchemaIds(type => jsonApiObjectSchemaSelector.GetSchemaId(type));
        }

        private static void AddCustomSwaggerComponents(IServiceCollection services)
        {
            services.AddSingleton<SwaggerGenerator>();
            services.AddSingleton<ISwaggerProvider, CachingSwaggerGenerator>();

            services.AddSingleton<ISerializerDataContractResolver, JsonApiDataContractResolver>();
        }

        private static void AddSwashbuckleCliCompatibility(IServiceScope scope, IMvcCoreBuilder mvcBuilder)
        {
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1957 for why this is needed.
            var routingConvention = scope.ServiceProvider.GetRequiredService<IJsonApiRoutingConvention>();
            mvcBuilder.AddMvcOptions(options => options.Conventions.Insert(0, routingConvention));
        }

        private static void AddOpenApiEndpointConvention(IServiceScope scope, IMvcCoreBuilder mvcBuilder)
        {
            var resourceGraph = scope.ServiceProvider.GetRequiredService<IResourceGraph>();
            var controllerResourceMapping = scope.ServiceProvider.GetRequiredService<IControllerResourceMapping>();

            mvcBuilder.AddMvcOptions(options => options.Conventions.Add(new OpenApiEndpointConvention(resourceGraph, controllerResourceMapping)));
        }
    }
}
