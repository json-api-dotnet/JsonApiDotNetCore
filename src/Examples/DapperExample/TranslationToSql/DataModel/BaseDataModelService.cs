using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Dapper;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.TranslationToSql.DataModel;

/// <summary>
/// Database-agnostic base type that infers additional information, based on foreign keys (provided by derived type) and the JSON:API resource graph.
/// </summary>
public abstract class BaseDataModelService : IDataModelService
{
    private readonly Dictionary<ResourceType, IReadOnlyDictionary<string, ResourceFieldAttribute?>> _columnMappingsByType = new();

    protected IResourceGraph ResourceGraph { get; }

    public abstract DatabaseProvider DatabaseProvider { get; }

    protected BaseDataModelService(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph);

        ResourceGraph = resourceGraph;
    }

    public abstract DbConnection CreateConnection();

    public abstract RelationshipForeignKey GetForeignKey(RelationshipAttribute relationship);

    protected void Initialize()
    {
        ScanColumnMappings();

        if (DatabaseProvider == DatabaseProvider.MySql)
        {
            // https://stackoverflow.com/questions/12510299/get-datetime-as-utc-with-dapper
            SqlMapper.AddTypeHandler(new DapperDateTimeOffsetHandlerForMySql());
        }
    }

    private void ScanColumnMappings()
    {
        foreach (ResourceType resourceType in ResourceGraph.GetResourceTypes())
        {
            _columnMappingsByType[resourceType] = ScanColumnMappings(resourceType);
        }
    }

    private IReadOnlyDictionary<string, ResourceFieldAttribute?> ScanColumnMappings(ResourceType resourceType)
    {
        Dictionary<string, ResourceFieldAttribute?> mappings = new();

        foreach (PropertyInfo property in resourceType.ClrType.GetProperties())
        {
            if (!IsMapped(property))
            {
                continue;
            }

            string columnName = property.Name;
            ResourceFieldAttribute? field = null;

            RelationshipAttribute? relationship = resourceType.FindRelationshipByPropertyName(property.Name);

            if (relationship != null)
            {
                RelationshipForeignKey foreignKey = GetForeignKey(relationship);

                if (!foreignKey.IsAtLeftSide)
                {
                    continue;
                }

                field = relationship;
                columnName = foreignKey.ColumnName;
            }
            else
            {
                AttrAttribute? attribute = resourceType.FindAttributeByPropertyName(property.Name);

                if (attribute != null)
                {
                    field = attribute;
                }
            }

            mappings[columnName] = field;
        }

        return mappings;
    }

    private static bool IsMapped(PropertyInfo property)
    {
        return property.GetCustomAttribute<NotMappedAttribute>() == null;
    }

    public IReadOnlyDictionary<string, ResourceFieldAttribute?> GetColumnMappings(ResourceType resourceType)
    {
        if (_columnMappingsByType.TryGetValue(resourceType, out IReadOnlyDictionary<string, ResourceFieldAttribute?>? columnMappings))
        {
            return columnMappings;
        }

        throw new InvalidOperationException($"Column mappings for resource type '{resourceType.ClrType.Name}' are unavailable.");
    }

    public object? GetColumnValue(ResourceType resourceType, IIdentifiable resource, string columnName)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(resource);
        AssertSameType(resourceType, resource);
        ArgumentGuard.NotNullNorEmpty(columnName);

        IReadOnlyDictionary<string, ResourceFieldAttribute?> columnMappings = GetColumnMappings(resourceType);

        if (!columnMappings.TryGetValue(columnName, out ResourceFieldAttribute? field))
        {
            throw new InvalidOperationException($"Column '{columnName}' not found on resource type '{resourceType}'.");
        }

        if (field is AttrAttribute attribute)
        {
            return attribute.GetValue(resource);
        }

        if (field is RelationshipAttribute relationship)
        {
            var rightResource = (IIdentifiable?)relationship.GetValue(resource);

            if (rightResource == null)
            {
                return null;
            }

            PropertyInfo rightKeyProperty = rightResource.GetClrType().GetProperty(TableSourceNode.IdColumnName)!;
            return rightKeyProperty.GetValue(rightResource);
        }

        PropertyInfo property = resourceType.ClrType.GetProperty(columnName)!;
        return property.GetValue(resource);
    }

    private static void AssertSameType(ResourceType resourceType, IIdentifiable resource)
    {
        Type declaredType = resourceType.ClrType;
        Type instanceType = resource.GetClrType();

        if (instanceType != declaredType)
        {
            throw new ArgumentException($"Expected resource of type '{declaredType.Name}' instead of '{instanceType.Name}'.", nameof(resource));
        }
    }

    public abstract bool IsColumnNullable(AttrAttribute attribute);

    private sealed class DapperDateTimeOffsetHandlerForMySql : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value;
        }

        public override DateTimeOffset Parse(object value)
        {
            return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }
}
