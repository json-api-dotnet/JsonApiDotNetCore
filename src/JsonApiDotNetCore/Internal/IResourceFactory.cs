using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Internal
{
    public interface IResourceFactory
    {
        public object CreateInstance(Type resourceType);
        public TResource CreateInstance<TResource>();
        public NewExpression CreateNewExpression(Type resourceType);
    }

    internal sealed class DefaultResourceFactory : IResourceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResourceFactory(IServiceProvider serviceProvider)
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
                var constructorArgument =
                    ActivatorUtilities.CreateInstance(_serviceProvider, constructorParameter.ParameterType);
                constructorArguments.Add(Expression.Constant(constructorArgument));
            }

            return Expression.New(longestConstructor, constructorArguments);
        }
    }
}
