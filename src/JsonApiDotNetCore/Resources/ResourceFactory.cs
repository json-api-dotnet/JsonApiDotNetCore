using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    internal sealed class ResourceFactory : IResourceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ResourceFactory(IServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IIdentifiable CreateInstance(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            return InnerCreateInstance(resourceType, _serviceProvider);
        }

        /// <inheritdoc />
        public TResource CreateInstance<TResource>()
            where TResource : IIdentifiable
        {
            return (TResource)InnerCreateInstance(typeof(TResource), _serviceProvider);
        }

        private static IIdentifiable InnerCreateInstance(Type type, IServiceProvider serviceProvider)
        {
            bool hasSingleConstructorWithoutParameters = HasSingleConstructorWithoutParameters(type);

            try
            {
                return hasSingleConstructorWithoutParameters
                    ? (IIdentifiable)Activator.CreateInstance(type)
                    : (IIdentifiable)ActivatorUtilities.CreateInstance(serviceProvider, type);
            }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            {
                throw new InvalidOperationException(
                    hasSingleConstructorWithoutParameters
                        ? $"Failed to create an instance of '{type.FullName}' using its default constructor."
                        : $"Failed to create an instance of '{type.FullName}' using injected constructor parameters.", exception);
            }
        }

        /// <inheritdoc />
        public NewExpression CreateNewExpression(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (HasSingleConstructorWithoutParameters(resourceType))
            {
                return Expression.New(resourceType);
            }

            var constructorArguments = new List<Expression>();

            ConstructorInfo longestConstructor = GetLongestConstructor(resourceType);

            foreach (ParameterInfo constructorParameter in longestConstructor.GetParameters())
            {
                try
                {
                    object constructorArgument = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, constructorParameter.ParameterType);

                    Expression argumentExpression = CreateTupleAccessExpressionForConstant(constructorArgument, constructorArgument.GetType());

                    constructorArguments.Add(argumentExpression);
                }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
                catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
                {
                    throw new InvalidOperationException(
                        $"Failed to create an instance of '{resourceType.FullName}': Parameter '{constructorParameter.Name}' could not be resolved.",
                        exception);
                }
            }

            return Expression.New(longestConstructor, constructorArguments);
        }

        private static Expression CreateTupleAccessExpressionForConstant(object value, Type type)
        {
            MethodInfo tupleCreateMethod = typeof(Tuple).GetMethods()
                .Single(method => method.Name == "Create" && method.IsGenericMethod && method.GetGenericArguments().Length == 1);

            MethodInfo constructedTupleCreateMethod = tupleCreateMethod.MakeGenericMethod(type);

            ConstantExpression constantExpression = Expression.Constant(value, type);

            MethodCallExpression tupleCreateCall = Expression.Call(constructedTupleCreateMethod, constantExpression);
            return Expression.Property(tupleCreateCall, "Item1");
        }

        internal static bool HasSingleConstructorWithoutParameters(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors().Where(constructor => !constructor.IsStatic).ToArray();

            return constructors.Length == 1 && constructors[0].GetParameters().Length == 0;
        }

        private static ConstructorInfo GetLongestConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors().Where(constructor => !constructor.IsStatic).ToArray();

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"No public constructor was found for '{type.FullName}'.");
            }

            ConstructorInfo bestMatch = constructors[0];
            int maxParameterLength = constructors[0].GetParameters().Length;

            for (int index = 1; index < constructors.Length; index++)
            {
                ConstructorInfo constructor = constructors[index];
                int length = constructor.GetParameters().Length;

                if (length > maxParameterLength)
                {
                    bestMatch = constructor;
                    maxParameterLength = length;
                }
            }

            return bestMatch;
        }
    }
}
