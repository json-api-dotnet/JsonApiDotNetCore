using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

/// <summary>
/// For schema generation, we rely on <see cref="JsonSerializerDataContractResolver" /> from Swashbuckle for all but our own JSON:API types.
/// </summary>
internal sealed class JsonApiDataContractResolver : ISerializerDataContractResolver
{
    private readonly JsonSerializerDataContractResolver _dataContractResolver;
    private readonly IResourceGraph _resourceGraph;

    public JsonApiDataContractResolver(IResourceGraph resourceGraph, IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);

        _resourceGraph = resourceGraph;
        _dataContractResolver = new JsonSerializerDataContractResolver(options.SerializerOptions);
    }

    public DataContract GetDataContractForType(Type type)
    {
        ArgumentGuard.NotNull(type);

        if (type == typeof(IIdentifiable))
        {
            // We have no way of telling Swashbuckle to opt out on this type, the closest we can get is return a contract with type Unknown.
            return DataContract.ForDynamic(typeof(object));
        }

        DataContract dataContract = _dataContractResolver.GetDataContractForType(type);

        IList<DataProperty>? replacementProperties = null;

        if (type.IsAssignableTo(typeof(IIdentifiable)))
        {
            replacementProperties = GetDataPropertiesThatExistInResourceClrType(type, dataContract);
        }

        if (replacementProperties != null)
        {
            dataContract = ReplacePropertiesInDataContract(dataContract, replacementProperties);
        }

        dataContract = OrderPropertiesInDataContract(dataContract);

        return dataContract;
    }

    private static DataContract ReplacePropertiesInDataContract(DataContract dataContract, IEnumerable<DataProperty> dataProperties)
    {
        return DataContract.ForObject(dataContract.UnderlyingType, dataProperties, dataContract.ObjectExtensionDataType, dataContract.ObjectTypeNameProperty,
            dataContract.ObjectTypeNameValue);
    }

    private static DataContract OrderPropertiesInDataContract(DataContract dataContract)
    {
#if NET6_0
        // From https://learn.microsoft.com/en-us/dotnet/api/system.type.getproperties#system-type-getproperties:
        //   In .NET 6 and earlier versions, the GetProperties method does not return properties in a particular order, such as alphabetical or declaration
        //   order. Your code must not depend on the order in which properties are returned, because that order varies. However, starting with .NET 7, the
        //   ordering is deterministic based upon the metadata ordering in the assembly.

        if (dataContract.ObjectProperties != null)
        {
            DataProperty[] dataPropertiesInOrder = dataContract.ObjectProperties.OrderBy(dataProperty => dataProperty.MemberInfo.MetadataToken).ToArray();

            // @formatter:keep_existing_linebreaks true

            return DataContract.ForObject(dataContract.UnderlyingType, dataPropertiesInOrder, dataContract.ObjectExtensionDataType,
                dataContract.ObjectTypeNameProperty, dataContract.ObjectTypeNameValue);

            // @formatter:keep_existing_linebreaks restore
        }
#endif
        return dataContract;
    }

    private List<DataProperty> GetDataPropertiesThatExistInResourceClrType(Type resourceClrType, DataContract dataContract)
    {
        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);
        List<DataProperty> dataProperties = [];

        foreach (DataProperty property in dataContract.ObjectProperties)
        {
            if (property.MemberInfo.Name == nameof(Identifiable<object>.Id))
            {
                // Schemas of JsonApiDotNetCore resources will obtain an Id property through inheritance of a resource identifier type.
                continue;
            }

            ResourceFieldAttribute? matchingField = resourceType.Fields.SingleOrDefault(field =>
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
        return new DataProperty(name, property.MemberType, property.IsRequired, property.IsNullable, property.IsReadOnly, property.IsWriteOnly,
            property.MemberInfo);
    }

    private static bool IsPropertyCompatibleWithMember(PropertyInfo property, MemberInfo member)
    {
        // In JsonApiDotNetCore the PropertyInfo for Id stored in AttrAttribute is that of the ReflectedType, whereas Newtonsoft uses the DeclaringType.
        return property == member || property.DeclaringType?.GetProperty(property.Name) == member;
    }
}
