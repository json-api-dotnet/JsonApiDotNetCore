using Dapper;
using DapperExample.TranslationToSql.Builders;
using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Repositories;

/// <summary>
/// Constructs Dapper <see cref="CommandDefinition" />s from SQL trees and handles order of updates.
/// </summary>
internal sealed class DapperFacade
{
    private readonly IDataModelService _dataModelService;

    public DapperFacade(IDataModelService dataModelService)
    {
        ArgumentGuard.NotNull(dataModelService);

        _dataModelService = dataModelService;
    }

    public CommandDefinition GetSqlCommand(SqlTreeNode node, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(node);

        var queryBuilder = new SqlQueryBuilder(_dataModelService.DatabaseProvider);
        string statement = queryBuilder.GetCommand(node);
        IDictionary<string, object?> parameters = queryBuilder.Parameters;

        return new CommandDefinition(statement, parameters, cancellationToken: cancellationToken);
    }

    public IReadOnlyCollection<CommandDefinition> BuildSqlCommandsForOneToOneRelationshipsChangedToNotNull(ResourceChangeDetector changeDetector,
        CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(changeDetector);

        List<CommandDefinition> sqlCommands = [];

        foreach ((HasOneAttribute relationship, (object? currentRightId, object newRightId)) in changeDetector.GetOneToOneRelationshipsChangedToNotNull())
        {
            // To prevent a unique constraint violation on the foreign key, first detach/delete the other row pointing to us, if any.
            // See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.

            RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(relationship);

            ResourceType resourceType = foreignKey.IsAtLeftSide ? relationship.LeftType : relationship.RightType;
            string whereColumnName = foreignKey.IsAtLeftSide ? foreignKey.ColumnName : TableSourceNode.IdColumnName;
            object? whereValue = foreignKey.IsAtLeftSide ? newRightId : currentRightId;

            if (whereValue == null)
            {
                // Creating new resource, so there can't be any existing FKs in other resources that are already pointing to us.
                continue;
            }

            if (foreignKey.IsNullable)
            {
                var updateBuilder = new UpdateClearOneToOneStatementBuilder(_dataModelService);
                UpdateNode updateNode = updateBuilder.Build(resourceType, foreignKey.ColumnName, whereColumnName, whereValue);
                CommandDefinition sqlCommand = GetSqlCommand(updateNode, cancellationToken);
                sqlCommands.Add(sqlCommand);
            }
            else
            {
                var deleteBuilder = new DeleteOneToOneStatementBuilder(_dataModelService);
                DeleteNode deleteNode = deleteBuilder.Build(resourceType, whereColumnName, whereValue);
                CommandDefinition sqlCommand = GetSqlCommand(deleteNode, cancellationToken);
                sqlCommands.Add(sqlCommand);
            }
        }

        return sqlCommands;
    }

    public IReadOnlyCollection<CommandDefinition> BuildSqlCommandsForChangedRelationshipsHavingForeignKeyAtRightSide<TId>(ResourceChangeDetector changeDetector,
        TId leftId, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(changeDetector);

        List<CommandDefinition> sqlCommands = [];

        foreach ((HasOneAttribute hasOneRelationship, (object? currentRightId, object? newRightId)) in changeDetector
            .GetChangedToOneRelationshipsWithForeignKeyAtRightSide())
        {
            RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(hasOneRelationship);

            var columnsToUpdate = new Dictionary<string, object?>
            {
                [foreignKey.ColumnName] = newRightId == null ? null : leftId
            };

            var updateBuilder = new UpdateResourceStatementBuilder(_dataModelService);
            UpdateNode updateNode = updateBuilder.Build(hasOneRelationship.RightType, columnsToUpdate, (newRightId ?? currentRightId)!);
            CommandDefinition sqlCommand = GetSqlCommand(updateNode, cancellationToken);
            sqlCommands.Add(sqlCommand);
        }

        foreach ((HasManyAttribute hasManyRelationship, (ISet<object> currentRightIds, ISet<object> newRightIds)) in changeDetector
            .GetChangedToManyRelationships())
        {
            RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(hasManyRelationship);

            object[] rightIdsToRemove = currentRightIds.Except(newRightIds).ToArray();
            object[] rightIdsToAdd = newRightIds.Except(currentRightIds).ToArray();

            if (rightIdsToRemove.Length > 0)
            {
                CommandDefinition sqlCommand = BuildSqlCommandForRemoveFromToMany(foreignKey, rightIdsToRemove, cancellationToken);
                sqlCommands.Add(sqlCommand);
            }

            if (rightIdsToAdd.Length > 0)
            {
                CommandDefinition sqlCommand = BuildSqlCommandForAddToToMany(foreignKey, leftId!, rightIdsToAdd, cancellationToken);
                sqlCommands.Add(sqlCommand);
            }
        }

        return sqlCommands;
    }

    public CommandDefinition BuildSqlCommandForRemoveFromToMany(RelationshipForeignKey foreignKey, object[] rightResourceIdValues,
        CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(foreignKey);
        ArgumentGuard.NotNullNorEmpty(rightResourceIdValues);

        if (!foreignKey.IsNullable)
        {
            var deleteBuilder = new DeleteResourceStatementBuilder(_dataModelService);
            DeleteNode deleteNode = deleteBuilder.Build(foreignKey.Relationship.RightType, rightResourceIdValues);
            return GetSqlCommand(deleteNode, cancellationToken);
        }

        var columnsToUpdate = new Dictionary<string, object?>
        {
            [foreignKey.ColumnName] = null
        };

        var updateBuilder = new UpdateResourceStatementBuilder(_dataModelService);
        UpdateNode updateNode = updateBuilder.Build(foreignKey.Relationship.RightType, columnsToUpdate, rightResourceIdValues);
        return GetSqlCommand(updateNode, cancellationToken);
    }

    public CommandDefinition BuildSqlCommandForAddToToMany(RelationshipForeignKey foreignKey, object leftId, object[] rightResourceIdValues,
        CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(foreignKey);
        ArgumentGuard.NotNull(leftId);
        ArgumentGuard.NotNullNorEmpty(rightResourceIdValues);

        var columnsToUpdate = new Dictionary<string, object?>
        {
            [foreignKey.ColumnName] = leftId
        };

        var updateBuilder = new UpdateResourceStatementBuilder(_dataModelService);
        UpdateNode updateNode = updateBuilder.Build(foreignKey.Relationship.RightType, columnsToUpdate, rightResourceIdValues);
        return GetSqlCommand(updateNode, cancellationToken);
    }

    public CommandDefinition BuildSqlCommandForCreate(ResourceChangeDetector changeDetector, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(changeDetector);

        IReadOnlyDictionary<string, object?> columnsToSet = changeDetector.GetChangedColumnValues();

        var insertBuilder = new InsertStatementBuilder(_dataModelService);
        InsertNode insertNode = insertBuilder.Build(changeDetector.ResourceType, columnsToSet);
        return GetSqlCommand(insertNode, cancellationToken);
    }

    public CommandDefinition? BuildSqlCommandForUpdate<TId>(ResourceChangeDetector changeDetector, TId leftId, CancellationToken cancellationToken)
    {
        ArgumentGuard.NotNull(changeDetector);

        IReadOnlyDictionary<string, object?> columnsToUpdate = changeDetector.GetChangedColumnValues();

        if (columnsToUpdate.Count > 0)
        {
            var updateBuilder = new UpdateResourceStatementBuilder(_dataModelService);
            UpdateNode updateNode = updateBuilder.Build(changeDetector.ResourceType, columnsToUpdate, leftId!);
            return GetSqlCommand(updateNode, cancellationToken);
        }

        return null;
    }
}
