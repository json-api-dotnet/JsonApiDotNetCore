using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Configuration;

internal sealed partial class EntityFrameworkCoreModelValidator
{
    private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

    private const string IdentifyingForeignKeyMessageTemplate =
        "Relationship '{RelationshipName}' uses an identifying foreign key where '{DependentType}.{ForeignKeyProperties}' is both " +
        "the primary key and the foreign key to '{PrincipalType}'. In JSON:API, resources must have independent identities. " +
        "Using an identifying foreign key prevents reassigning the relationship to another resource (because primary keys cannot be updated) " +
        "and prevents clearing the relationship. To fix this, define a dedicated foreign key column instead, for example: " +
        "builder.Entity<{DependentType}>().HasOne(...).WithOne(...).HasForeignKey<{DependentType}>(\"{ForeignKeyName}\"). " +
        "Note that doing so results in database changes.";

    private const string ClientSetNullMessageTemplate =
        "{TargetDescription} uses DeleteBehavior.ClientSetNull, which causes the database foreign key to be created with NO ACTION. " +
        "When deleting a '{PrincipalType}' resource, JsonApiDotNetCore is forced to query and load referencing entities into memory " +
        "so Entity Framework Core can set their foreign keys to NULL. To delegate clearing foreign keys directly to the database engine " +
        "and avoid executing extra database queries prior to deletion, configure .OnDelete(DeleteBehavior.SetNull) in OnModelCreating.";

    private const string MessageSuffixInException = """

        To log a warning instead of throwing this exception, set options.ThrowForProblematicEntityMappings to false.
        """;

    private readonly IReadOnlyCollection<DbContext> _dbContexts;
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly ILogger<EntityFrameworkCoreModelValidator> _logger;

    public EntityFrameworkCoreModelValidator(IReadOnlyCollection<DbContext> dbContexts, IJsonApiOptions options, IResourceGraph resourceGraph,
        ILogger<EntityFrameworkCoreModelValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContexts);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContexts = dbContexts;
        _options = options;
        _resourceGraph = resourceGraph;
        _logger = logger;
    }

    public void Validate()
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            foreach (DbContext dbContext in _dbContexts)
            {
                Validate(dbContext);
            }
        }
    }

    private void Validate(DbContext dbContext)
    {
        ValidateIdentifyingForeignKeys(dbContext.Model);
        ValidateClientSetNull(dbContext);
    }

    private void ValidateIdentifyingForeignKeys(IReadOnlyModel model)
    {
        var seenForeignKeys = new HashSet<IReadOnlyForeignKey>();

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes().Where(resourceType => resourceType.Relationships.Count > 0))
        {
            IReadOnlyEntityType? entityType = model.FindEntityType(resourceType.ClrType);

            if (entityType != null)
            {
                ValidateIdentifyingForeignKeys(entityType, resourceType, seenForeignKeys);
            }
        }
    }

    private void ValidateIdentifyingForeignKeys(IReadOnlyEntityType entityType, ResourceType resourceType, HashSet<IReadOnlyForeignKey> seenForeignKeys)
    {
        foreach (RelationshipAttribute relationship in resourceType.Relationships)
        {
            if (relationship is HasOneAttribute)
            {
                IReadOnlyNavigation? navigation = entityType.FindNavigation(relationship.Property.Name);

                if (navigation != null && seenForeignKeys.Add(navigation.ForeignKey))
                {
                    ValidateIdentifyingForeignKey(navigation.ForeignKey, relationship);
                }
            }
        }
    }

    private void ValidateIdentifyingForeignKey(IReadOnlyForeignKey foreignKey, RelationshipAttribute relationship)
    {
        if (!foreignKey.IsOwnership)
        {
            IReadOnlyKey? primaryKey = foreignKey.DeclaringEntityType.FindPrimaryKey();

            if (primaryKey != null && foreignKey.Properties.All(property => primaryKey.Properties.Contains(property)))
            {
                string relationshipName = $"{relationship.LeftType.ClrType.Name}.{relationship.Property.Name}";
                string dependentTypeName = foreignKey.DeclaringEntityType.ClrType.Name;
                string principalTypeName = foreignKey.PrincipalEntityType.ClrType.Name;
                string foreignKeyPropertyNames = string.Join(", ", foreignKey.Properties.Select(property => property.Name));
                string foreignKeyName = $"{principalTypeName}Id";

                if (_options.ThrowForProblematicEntityMappings)
                {
                    string message = FormatIdentifyingForeignKeyMessage(relationshipName, dependentTypeName, foreignKeyPropertyNames, principalTypeName,
                        foreignKeyName);

                    throw new InvalidConfigurationException(message);
                }

                LogIdentifyingForeignKey(relationshipName, dependentTypeName, foreignKeyPropertyNames, principalTypeName, foreignKeyName);
            }
        }
    }

    private void ValidateClientSetNull(DbContext dbContext)
    {
        if (!IsSqlServer(dbContext))
        {
            var seenForeignKeys = new HashSet<IForeignKey>();

            foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
            {
                IEntityType? entityType = dbContext.Model.FindEntityType(resourceType.ClrType);

                if (entityType != null)
                {
                    ValidateClientSetNull(entityType, resourceType, seenForeignKeys);
                }
            }
        }
    }

    private static bool IsSqlServer(DbContext dbContext)
    {
        return string.Equals(dbContext.Database.ProviderName, SqlServerProviderName, StringComparison.Ordinal);
    }

    private void ValidateClientSetNull(IEntityType principalEntityType, ResourceType resourceType, HashSet<IForeignKey> seenForeignKeys)
    {
        foreach (IForeignKey foreignKey in principalEntityType.GetReferencingForeignKeys())
        {
            if (foreignKey.DeleteBehavior == DeleteBehavior.ClientSetNull && seenForeignKeys.Add(foreignKey))
            {
                string targetDescription = GetClientSetNullTargetDescription(foreignKey, resourceType);

                if (_options.ThrowForProblematicEntityMappings)
                {
                    string message = FormatClientSetNullMessage(targetDescription, resourceType.ClrType.Name);
                    throw new InvalidConfigurationException(message);
                }

                LogClientSetNull(targetDescription, resourceType.ClrType.Name);
            }
        }
    }

    private string GetClientSetNullTargetDescription(IForeignKey foreignKey, ResourceType principalResourceType)
    {
        Type dependentClrType = foreignKey.DeclaringEntityType.ClrType;
        ResourceType? dependentResourceType = _resourceGraph.FindResourceType(dependentClrType);

        if (dependentResourceType != null && foreignKey.DependentToPrincipal != null)
        {
            RelationshipAttribute? relationship = dependentResourceType.FindRelationshipByPropertyName(foreignKey.DependentToPrincipal.Name);

            if (relationship != null)
            {
                return $"Relationship '{dependentResourceType.ClrType.Name}.{relationship.Property.Name}'";
            }
        }

        if (foreignKey.PrincipalToDependent != null)
        {
            RelationshipAttribute? relationship = principalResourceType.FindRelationshipByPropertyName(foreignKey.PrincipalToDependent.Name);

            if (relationship != null)
            {
                return $"Relationship '{principalResourceType.ClrType.Name}.{relationship.Property.Name}'";
            }
        }

        return $"Foreign key on '{dependentClrType.Name}' referencing resource '{principalResourceType.ClrType.Name}'";
    }

    private static string FormatIdentifyingForeignKeyMessage(string relationshipName, string dependentType, string foreignKeyProperties, string principalType,
        string foreignKeyName)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string message = IdentifyingForeignKeyMessageTemplate
            .Replace("{RelationshipName}", relationshipName, StringComparison.Ordinal)
            .Replace("{DependentType}", dependentType, StringComparison.Ordinal)
            .Replace("{ForeignKeyProperties}", foreignKeyProperties, StringComparison.Ordinal)
            .Replace("{PrincipalType}", principalType, StringComparison.Ordinal)
            .Replace("{ForeignKeyName}", foreignKeyName, StringComparison.Ordinal);

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        return $"{message}{MessageSuffixInException}";
    }

    private static string FormatClientSetNullMessage(string targetDescription, string principalType)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string message = ClientSetNullMessageTemplate
            .Replace("{TargetDescription}", targetDescription, StringComparison.Ordinal)
            .Replace("{PrincipalType}", principalType, StringComparison.Ordinal);

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        return $"{message}{MessageSuffixInException}";
    }

    [LoggerMessage(Level = LogLevel.Warning, SkipEnabledCheck = true, Message = IdentifyingForeignKeyMessageTemplate)]
    private partial void LogIdentifyingForeignKey(string relationshipName, string dependentType, string foreignKeyProperties, string principalType,
        string foreignKeyName);

    [LoggerMessage(Level = LogLevel.Warning, SkipEnabledCheck = true, Message = ClientSetNullMessageTemplate)]
    private partial void LogClientSetNull(string targetDescription, string principalType);
}
