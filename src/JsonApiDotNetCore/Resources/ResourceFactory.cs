using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc />
internal sealed class ResourceFactory : IResourceFactory
{
    private static readonly TypeLocator TypeLocator = new();

    private readonly IServiceProvider _serviceProvider;

    public ResourceFactory(IServiceProvider serviceProvider)
    {
        ArgumentGuard.NotNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IIdentifiable CreateInstance(Type resourceClrType)
    {
        ArgumentGuard.NotNull(resourceClrType);

        if (!resourceClrType.IsAssignableTo(typeof(IIdentifiable)))
        {
            throw new InvalidOperationException($"Resource type '{resourceClrType}' does not implement IIdentifiable.");
        }

        if (resourceClrType.IsAbstract)
        {
            return CreateWrapperForAbstractType(resourceClrType);
        }

        return InnerCreateInstance(resourceClrType, _serviceProvider);
    }

    private static IIdentifiable CreateWrapperForAbstractType(Type resourceClrType)
    {
        ResourceDescriptor? descriptor = TypeLocator.ResolveResourceDescriptor(resourceClrType);

        if (descriptor == null)
        {
            throw new InvalidOperationException($"Resource type '{resourceClrType}' implements 'IIdentifiable', but not 'IIdentifiable<TId>'.");
        }

        Type wrapperClrType = typeof(AbstractResourceWrapper<>).MakeGenericType(descriptor.IdClrType);
        ConstructorInfo constructor = wrapperClrType.GetConstructors().Single();

        object resource = constructor.Invoke(ArrayFactory.Create<object>(resourceClrType));
        return (IIdentifiable)resource;
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
                ? (IIdentifiable)Activator.CreateInstance(type)!
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
    public NewExpression CreateNewExpression(Type resourceClrType)
    {
        ArgumentGuard.NotNull(resourceClrType);

        if (HasSingleConstructorWithoutParameters(resourceClrType))
        {
            return Expression.New(resourceClrType);
        }

        var constructorArguments = new List<Expression>();

        ConstructorInfo longestConstructor = GetLongestConstructor(resourceClrType);

        foreach (ParameterInfo constructorParameter in longestConstructor.GetParameters())
        {
            try
            {
                object constructorArgument = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, constructorParameter.ParameterType);

                Expression argumentExpression = constructorArgument.CreateTupleAccessExpressionForConstant(constructorArgument.GetType());
                constructorArguments.Add(argumentExpression);
            }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of '{resourceClrType.FullName}': Parameter '{constructorParameter.Name}' could not be resolved.", exception);
            }
        }

        return Expression.New(longestConstructor, constructorArguments);
    }

    private static bool HasSingleConstructorWithoutParameters(Type type)
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
