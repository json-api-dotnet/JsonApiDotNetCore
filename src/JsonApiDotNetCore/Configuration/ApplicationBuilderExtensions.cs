using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
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
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            
            using var scope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var inverseRelationshipResolver = scope.ServiceProvider.GetRequiredService<IInverseRelationships>();
            inverseRelationshipResolver.Resolve();
            
            var jsonApiApplicationBuilder =  builder.ApplicationServices.GetRequiredService<IJsonApiApplicationBuilder>();
            jsonApiApplicationBuilder.ConfigureMvcOptions = options =>
            {
                var inputFormatter = builder.ApplicationServices.GetRequiredService<IJsonApiInputFormatter>();
                options.InputFormatters.Insert(0, inputFormatter);

                var outputFormatter = builder.ApplicationServices.GetRequiredService<IJsonApiOutputFormatter>();
                options.OutputFormatters.Insert(0, outputFormatter);

                var routingConvention = builder.ApplicationServices.GetRequiredService<IJsonApiRoutingConvention>();
                options.Conventions.Insert(0, routingConvention);
                
                // var interceptor = new MetadataDetailsProviderListInterceptor(options.ModelMetadataDetailsProviders);
                // var property = typeof(MvcOptions).GetField("<ModelMetadataDetailsProviders>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                // property.SetValue(options, interceptor);

                options.ModelValidatorProviders[0] = new JsonApiModelValidationProvider(options.ModelValidatorProviders[0]);
            };

            builder.UseMiddleware<JsonApiMiddleware>();
        }
    }

    internal sealed class MetadataDetailsProviderListInterceptor : IList<IMetadataDetailsProvider>
    {
        private readonly IList<IMetadataDetailsProvider> _list;

        public MetadataDetailsProviderListInterceptor(IList<IMetadataDetailsProvider> list)
        {
            _list = list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IMetadataDetailsProvider item)
        {
            
            if (item is IBindingMetadataProvider && item is IDisplayMetadataProvider && item is IValidationMetadataProvider)
            {
                _list.Add(new JsonApiMetadataProvider(item));
            }
            
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(IMetadataDetailsProvider item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(IMetadataDetailsProvider[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(IMetadataDetailsProvider item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => _list.IsReadOnly;

        public IEnumerator<IMetadataDetailsProvider> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IMetadataDetailsProvider item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, IMetadataDetailsProvider item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public IMetadataDetailsProvider this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
    }
}
