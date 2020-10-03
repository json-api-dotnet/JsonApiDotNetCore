using System;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Registers the JsonApiDotNetCore middleware.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <example>
        /// The code below is the minimal that is required for proper activation,
        /// which should be added to your Startup.Configure method.
        /// <code><![CDATA[
        /// app.UseRouting();
        /// app.UseJsonApi();
        /// app.UseEndpoints(endpoints => endpoints.MapControllers());
        /// ]]></code>
        /// </example>
        public static void UseJsonApi(this IApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            using var scope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var inverseRelationshipResolver = scope.ServiceProvider.GetRequiredService<IInverseRelationships>();
            inverseRelationshipResolver.Resolve();

            var jsonApiApplicationBuilder = builder.ApplicationServices.GetRequiredService<IJsonApiApplicationBuilder>();
            jsonApiApplicationBuilder.ConfigureMvcOptions = options =>
            {
                var inputFormatter = builder.ApplicationServices.GetRequiredService<IJsonApiInputFormatter>();
                options.InputFormatters.Insert(0, inputFormatter);

                var outputFormatter = builder.ApplicationServices.GetRequiredService<IJsonApiOutputFormatter>();
                options.OutputFormatters.Insert(0, outputFormatter);

                var routingConvention = builder.ApplicationServices.GetRequiredService<IJsonApiRoutingConvention>();
                options.Conventions.Insert(0, routingConvention);
                
                
                // var validationAttributeAdapterProvider = builder.ApplicationServices.GetRequiredService<IValidationAttributeAdapterProvider>();
                // var dataAnnotationLocalizationOptions = builder.ApplicationServices.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>();
                // var stringLocalizerFactory = builder.ApplicationServices.GetService<IStringLocalizerFactory>();
                // options.ModelValidatorProviders.Add(new DataAnnotationsModelValidatorProvider_COPY(validationAttributeAdapterProvider, dataAnnotationLocalizationOptions, stringLocalizerFactory));
                // options.ModelValidatorProviders.Add(new JsonApiModelValidationProvider());
                // options.ModelMetadataDetailsProviders.Add(new JsonApiDataAnnotationsMetadataProvider());

            };

            builder.UseMiddleware<JsonApiMiddleware>();
        }
    }
}



