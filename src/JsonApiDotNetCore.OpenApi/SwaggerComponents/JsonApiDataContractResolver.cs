using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    /// <summary>
    /// For schema generation, we rely on <see cref="NewtonsoftDataContractResolver" /> from Swashbuckle for all but our own JSON:API types.
    /// </summary>
    internal sealed class JsonApiDataContractResolver : ISerializerDataContractResolver
    {
        private readonly NewtonsoftDataContractResolver _dataContractResolver;
        private readonly IResourceContextProvider _resourceContextProvider;

        public JsonApiDataContractResolver(IResourceContextProvider resourceContextProvider, IJsonApiOptions jsonApiOptions)
        {
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(jsonApiOptions, nameof(jsonApiOptions));

            _resourceContextProvider = resourceContextProvider;

            JsonSerializerSettings serializerSettings = jsonApiOptions.SerializerSettings ?? new JsonSerializerSettings();
            _dataContractResolver = new NewtonsoftDataContractResolver(serializerSettings);
        }

        public DataContract GetDataContractForType(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            if (type == typeof(IIdentifiable))
            {
                // We have no way of telling Swashbuckle to opt out on this type, the closest we can get is return a contract with type Unknown.
                return DataContract.ForDynamic(typeof(object));
            }

            DataContract dataContract = _dataContractResolver.GetDataContractForType(type);

            IList<DataProperty> replacementProperties = null;

            if (type.IsAssignableTo(typeof(IIdentifiable)))
            {
                replacementProperties = GetDataPropertiesThatExistInResourceContext(type, dataContract);
            }

            if (replacementProperties != null)
            {
                dataContract = ReplacePropertiesInDataContract(dataContract, replacementProperties);
            }

            return dataContract;
        }

        private static bool IsIdentifiableBaseType(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Identifiable<>);
            }

            return type == typeof(Identifiable);
        }

        private static bool IsIdentity(DataProperty property)
        {
            return property.MemberInfo.Name == nameof(Identifiable.Id);
        }

        private static DataContract ReplacePropertiesInDataContract(DataContract dataContract, IEnumerable<DataProperty> dataProperties)
        {
            return DataContract.ForObject(dataContract.UnderlyingType, dataProperties, dataContract.ObjectExtensionDataType,
                dataContract.ObjectTypeNameProperty, dataContract.ObjectTypeNameValue);
        }

        private IList<DataProperty> GetDataPropertiesThatExistInResourceContext(Type resourceType, DataContract dataContract)
        {
            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            var dataProperties = new List<DataProperty>();

            foreach (DataProperty property in dataContract.ObjectProperties)
            {
                if (property.MemberInfo.Name == nameof(Identifiable.Id))
                {
                    // Schemas of JsonApiDotNetCore resources will obtain an Id property through inheritance of a resource identifier type.
                    continue;
                }

                ResourceFieldAttribute matchingField = resourceContext.Fields.SingleOrDefault(field =>
                    IsPropertyCompatibleWithMember(field.Property, property.MemberInfo));

                if (matchingField != null)
                {
                    DataProperty matchingProperty = matchingField.PublicName != property.Name
                        ? ChangeDataPropertyName(property, matchingField.PublicName)
                        : property;

                    dataProperties.Add(matchingProperty);
                }
            }

            return dataProperties;
        }

        private static DataProperty ChangeDataPropertyName(DataProperty property, string name)
        {
            return new(name, property.MemberType, property.IsRequired, property.IsNullable, property.IsReadOnly, property.IsWriteOnly, property.MemberInfo);
        }

        private static bool IsPropertyCompatibleWithMember(PropertyInfo property, MemberInfo member)
        {
            // In JsonApiDotNetCore the PropertyInfo for Id stored in AttrAttribute is that of the ReflectedType, whereas Newtonsoft uses the DeclaringType.
            return property == member || property.DeclaringType?.GetProperty(property.Name) == member;
        }
    }
}
