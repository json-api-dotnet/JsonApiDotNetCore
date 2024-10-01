using System.Data.Common;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MySqlConnector;
using Npgsql;

namespace DapperExample.TranslationToSql.DataModel;

/// <summary>
/// Derives foreign keys and connection strings from an existing Entity Framework Core model.
/// </summary>
public sealed class FromEntitiesDataModelService(IResourceGraph resourceGraph)
    : BaseDataModelService(resourceGraph)
{
    private readonly Dictionary<RelationshipAttribute, RelationshipForeignKey> _foreignKeysByRelationship = [];
    private readonly Dictionary<AttrAttribute, bool> _columnNullabilityPerAttribute = [];
    private string? _connectionString;
    private DatabaseProvider? _databaseProvider;

    public override DatabaseProvider DatabaseProvider => AssertHasDatabaseProvider();

    public void Initialize(DbContext dbContext)
    {
        _connectionString = dbContext.Database.GetConnectionString();

        _databaseProvider = dbContext.Database.ProviderName switch
        {
            "Npgsql.EntityFrameworkCore.PostgreSQL" => DatabaseProvider.PostgreSql,
            "Pomelo.EntityFrameworkCore.MySql" => DatabaseProvider.MySql,
            "Microsoft.EntityFrameworkCore.SqlServer" => DatabaseProvider.SqlServer,
            _ => throw new NotSupportedException($"Unknown database provider '{dbContext.Database.ProviderName}'.")
        };

        ScanForeignKeys(dbContext.Model);
        ScanColumnNullability(dbContext.Model);
        Initialize();
    }

    private void ScanForeignKeys(IReadOnlyModel entityModel)
    {
        foreach (RelationshipAttribute relationship in ResourceGraph.GetResourceTypes().SelectMany(resourceType => resourceType.Relationships))
        {
            IReadOnlyEntityType? leftEntityType = entityModel.FindEntityType(relationship.LeftType.ClrType);
            IReadOnlyNavigation? navigation = leftEntityType?.FindNavigation(relationship.Property.Name);

            if (navigation != null)
            {
                bool isAtLeftSide = navigation.ForeignKey.DeclaringEntityType.ClrType == relationship.LeftType.ClrType;
                string columnName = navigation.ForeignKey.Properties.Single().Name;
                bool isNullable = !navigation.ForeignKey.IsRequired;

                var foreignKey = new RelationshipForeignKey(DatabaseProvider, relationship, isAtLeftSide, columnName, isNullable);
                _foreignKeysByRelationship[relationship] = foreignKey;
            }
        }
    }

    private void ScanColumnNullability(IReadOnlyModel entityModel)
    {
        foreach (ResourceType resourceType in ResourceGraph.GetResourceTypes())
        {
            ScanColumnNullability(resourceType, entityModel);
        }
    }

    private void ScanColumnNullability(ResourceType resourceType, IReadOnlyModel entityModel)
    {
        IReadOnlyEntityType? entityType = entityModel.FindEntityType(resourceType.ClrType);

        if (entityType != null)
        {
            foreach (AttrAttribute attribute in resourceType.Attributes)
            {
                IReadOnlyProperty? property = entityType.FindProperty(attribute.Property.Name);

                if (property != null)
                {
                    _columnNullabilityPerAttribute[attribute] = property.IsNullable;
                }
            }
        }
    }

    public override DbConnection CreateConnection()
    {
        string connectionString = AssertHasConnectionString();
        DatabaseProvider databaseProvider = AssertHasDatabaseProvider();

        return databaseProvider switch
        {
            DatabaseProvider.PostgreSql => new NpgsqlConnection(connectionString),
            DatabaseProvider.MySql => new MySqlConnection(connectionString),
            DatabaseProvider.SqlServer => new SqlConnection(connectionString),
            _ => throw new NotSupportedException($"Unknown database provider '{databaseProvider}'.")
        };
    }

    public override RelationshipForeignKey GetForeignKey(RelationshipAttribute relationship)
    {
        if (_foreignKeysByRelationship.TryGetValue(relationship, out RelationshipForeignKey? foreignKey))
        {
            return foreignKey;
        }

        throw new InvalidOperationException(
            $"Foreign key mapping for relationship '{relationship.LeftType.ClrType.Name}.{relationship.Property.Name}' is unavailable.");
    }

    public override bool IsColumnNullable(AttrAttribute attribute)
    {
        if (_columnNullabilityPerAttribute.TryGetValue(attribute, out bool isNullable))
        {
            return isNullable;
        }

        throw new InvalidOperationException($"Attribute '{attribute}' is unavailable.");
    }

    private DatabaseProvider AssertHasDatabaseProvider()
    {
        if (_databaseProvider == null)
        {
            throw new InvalidOperationException($"Database provider is unavailable. Call {nameof(Initialize)} first.");
        }

        return _databaseProvider.Value;
    }

    private string AssertHasConnectionString()
    {
        if (_connectionString == null)
        {
            throw new InvalidOperationException($"Connection string is unavailable. Call {nameof(Initialize)} first.");
        }

        return _connectionString;
    }
}
