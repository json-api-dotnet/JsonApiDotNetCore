using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
            using var scope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var inverseRelationshipResolver = scope.ServiceProvider.GetRequiredService<IInverseRelationships>();
            inverseRelationshipResolver.Resolve();
            
            var jsonApiApplicationBuilder =  builder.ApplicationServices.GetRequiredService<IJsonApiApplicationBuilder>();
            jsonApiApplicationBuilder.ConfigureMvcOptions = options =>
            {
                options.InputFormatters.Insert(0, builder.ApplicationServices.GetRequiredService<IJsonApiInputFormatter>());
                options.OutputFormatters.Insert(0, builder.ApplicationServices.GetRequiredService<IJsonApiOutputFormatter>());
                options.Conventions.Insert(0, builder.ApplicationServices.GetRequiredService<IJsonApiRoutingConvention>());
            };
        }
    }
}
