using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Internal
{
    public interface IResourceFactory
    {
        public IIdentifiable CreateInstance(Type resourceType);
        public TResource CreateInstance<TResource>() where TResource : IIdentifiable;
    }

    internal sealed class DefaultResourceFactory : IResourceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResourceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IIdentifiable CreateInstance(Type resourceType)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            return (IIdentifiable) InnerCreateInstance(resourceType, _serviceProvider);
        }

        public TResource CreateInstance<TResource>() where TResource : IIdentifiable
        {
            return (TResource) InnerCreateInstance(typeof(TResource), _serviceProvider);
        }

        private static object InnerCreateInstance(Type type, IServiceProvider serviceProvider)
        {
            bool hasSingleConstructorWithoutParameters = type.HasSingleConstructorWithoutParameters();

            try
            {
                return hasSingleConstructorWithoutParameters
                    ? Activator.CreateInstance(type)
                    : ActivatorUtilities.CreateInstance(serviceProvider, type);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(hasSingleConstructorWithoutParameters
                        ? $"Failed to create an instance of '{type.FullName}' using its default constructor."
                        : $"Failed to create an instance of '{type.FullName}' using injected constructor parameters.",
                    exception);
            }
        }
    }
}
