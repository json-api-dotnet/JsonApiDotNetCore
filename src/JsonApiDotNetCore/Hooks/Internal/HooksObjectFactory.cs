using System;
using System.Reflection;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    internal sealed class HooksObjectFactory
    {
        /// <summary>
        /// Creates an instance of the specified generic type
        /// </summary>
        /// <returns>
        /// The instance of the parameterized generic type
        /// </returns>
        /// <param name="parameter">
        /// Generic type parameter to be used in open type.
        /// </param>
        /// <param name="constructorArguments">
        /// Constructor arguments to be provided in instantiation.
        /// </param>
        /// <param name="openType">
        /// Open generic type
        /// </param>
        public object CreateInstanceOfOpenType(Type openType, Type parameter, params object[] constructorArguments)
        {
            return CreateInstanceOfOpenType(openType, parameter.AsArray(), constructorArguments);
        }

        /// <summary>
        /// Use this overload if you need to instantiate a type that has an internal constructor
        /// </summary>
        public object CreateInstanceOfInternalOpenType(Type openType, Type parameter, params object[] constructorArguments)
        {
            Type[] parameters =
            {
                parameter
            };

            Type closedType = openType.MakeGenericType(parameters);
            return Activator.CreateInstance(closedType, BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArguments, null);
        }

        /// <summary>
        /// Creates an instance of the specified generic type
        /// </summary>
        /// <returns>
        /// The instance of the parameterized generic type
        /// </returns>
        /// <param name="parameters">
        /// Generic type parameters to be used in open type.
        /// </param>
        /// <param name="constructorArguments">
        /// Constructor arguments to be provided in instantiation.
        /// </param>
        /// <param name="openType">
        /// Open generic type
        /// </param>
        private object CreateInstanceOfOpenType(Type openType, Type[] parameters, params object[] constructorArguments)
        {
            Type closedType = openType.MakeGenericType(parameters);
            return Activator.CreateInstance(closedType, constructorArguments);
        }

        /// <summary>
        /// Gets the type (such as Guid or int) of the Id property on a type that implements <see cref="IIdentifiable" />.
        /// </summary>
        public Type GetIdType(Type resourceType)
        {
            PropertyInfo property = resourceType.GetProperty(nameof(Identifiable.Id));

            if (property == null)
            {
                throw new ArgumentException($"Type '{resourceType.Name}' does not have 'Id' property.");
            }

            return property.PropertyType;
        }
    }
}
