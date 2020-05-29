using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Internal
{
    public interface IResourceFactory
    {
        public object CreateInstance(Type resourceType);
        public TResource CreateInstance<TResource>();
        public NewExpression CreateNewExpression(Type resourceType);
    }

    internal sealed class ResourceFactory : IResourceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ResourceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object CreateInstance(Type resourceType)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            return InnerCreateInstance(resourceType, _serviceProvider);
        }

        public TResource CreateInstance<TResource>()
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

        public NewExpression CreateNewExpression(Type resourceType)
        {
            if (resourceType.HasSingleConstructorWithoutParameters())
            {
                return Expression.New(resourceType);
            }

            List<Expression> constructorArguments = new List<Expression>();

            var longestConstructor = resourceType.GetLongestConstructor();
            foreach (ParameterInfo constructorParameter in longestConstructor.GetParameters())
            {
                try
                {
                    object constructorArgument =
                        ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, constructorParameter.ParameterType);

                    constructorArguments.Add(Expression.Constant(constructorArgument));
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Failed to create an instance of '{resourceType.FullName}': Parameter '{constructorParameter.Name}' could not be resolved.",
                        exception);
                }
            }

            return Expression.New(longestConstructor, constructorArguments);
        }
    }
}
