using System;
using System.Collections.Generic;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
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

            AddJsonApiInputFormatterWorkaround(mvcBuilder);
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
        }

        private static void AddSwaggerGenerator(IServiceScope scope, IServiceCollection services, Action<SwaggerGenOptions> setupSwaggerGenAction)
        {
            var controllerResourceMapping = scope.ServiceProvider.GetRequiredService<IControllerResourceMapping>();
            var resourceContextProvider = scope.ServiceProvider.GetRequiredService<IResourceContextProvider>();
            var jsonApiOptions = scope.ServiceProvider.GetRequiredService<IJsonApiOptions>();
            NamingStrategy namingStrategy = ((DefaultContractResolver)jsonApiOptions.SerializerSettings.ContractResolver)!.NamingStrategy;

            AddSchemaGenerator(services);

            services.AddSwaggerGen(swaggerGenOptions =>
            {
                SetOperationInfo(swaggerGenOptions, controllerResourceMapping, resourceContextProvider, namingStrategy);
                SetSchemaIdSelector(swaggerGenOptions, resourceContextProvider, namingStrategy);
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
            IResourceContextProvider resourceContextProvider, NamingStrategy namingStrategy)
        {
            swaggerGenOptions.TagActionsBy(description => GetOperationTags(description, controllerResourceMapping, resourceContextProvider));

            JsonApiOperationIdSelector jsonApiOperationIdSelector = new(controllerResourceMapping, namingStrategy);
            swaggerGenOptions.CustomOperationIds(jsonApiOperationIdSelector.GetOperationId);
        }

        private static IList<string> GetOperationTags(ApiDescription description, IControllerResourceMapping controllerResourceMapping,
            IResourceContextProvider resourceContextProvider)
        {
            MethodInfo actionMethod = description.ActionDescriptor.GetActionMethod();
            Type resourceType = controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);
            ResourceContext resourceContext = resourceContextProvider.GetResourceContext(resourceType);

            return new[]
            {
                resourceContext.PublicName
            };
        }

        private static void SetSchemaIdSelector(SwaggerGenOptions swaggerGenOptions, IResourceContextProvider resourceContextProvider,
            NamingStrategy namingStrategy)
        {
            ResourceNameFormatterProxy resourceNameFormatter = new(namingStrategy);
            JsonApiSchemaIdSelector jsonApiObjectSchemaSelector = new(resourceNameFormatter, resourceContextProvider);

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
            var resourceContextProvider = scope.ServiceProvider.GetRequiredService<IResourceContextProvider>();
            var controllerResourceMapping = scope.ServiceProvider.GetRequiredService<IControllerResourceMapping>();

            mvcBuilder.AddMvcOptions(options => options.Conventions.Add(new OpenApiEndpointConvention(resourceContextProvider, controllerResourceMapping)));
        }

        private static void AddJsonApiInputFormatterWorkaround(IMvcCoreBuilder mvcBuilder)
        {
            // See https://github.com/json-api-dotnet/JsonApiDotNetCore/pull/972 for why this is needed.
            mvcBuilder.AddMvcOptions(options => options.InputFormatters.Add(new JsonApiInputFormatterWithMetadata()));
        }
    }
}
