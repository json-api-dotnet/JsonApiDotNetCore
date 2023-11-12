using System.Data.Common;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.TranslationToSql.DataModel;

/// <summary>
/// Provides information about the underlying database model, such as foreign key and column names.
/// </summary>
public interface IDataModelService
{
    DatabaseProvider DatabaseProvider { get; }

    DbConnection CreateConnection();

    RelationshipForeignKey GetForeignKey(RelationshipAttribute relationship);

    IReadOnlyDictionary<string, ResourceFieldAttribute?> GetColumnMappings(ResourceType resourceType);

    object? GetColumnValue(ResourceType resourceType, IIdentifiable resource, string columnName);

    bool IsColumnNullable(AttrAttribute attribute);
}
