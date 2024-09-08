using System.Collections.ObjectModel;
using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace DapperExample.TranslationToSql.Builders;

internal sealed class InsertStatementBuilder(IDataModelService dataModelService) : StatementBuilder(dataModelService)
{
    public InsertNode Build(ResourceType resourceType, IReadOnlyDictionary<string, object?> columnsToSet)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(columnsToSet);

        ResetState();

        TableNode table = GetTable(resourceType, null);
        ReadOnlyCollection<ColumnAssignmentNode> assignments = GetColumnAssignments(columnsToSet, table);

        return new InsertNode(table, assignments);
    }

    private ReadOnlyCollection<ColumnAssignmentNode> GetColumnAssignments(IReadOnlyDictionary<string, object?> columnsToSet, TableNode table)
    {
        List<ColumnAssignmentNode> assignments = [];
        ColumnNode idColumn = table.GetIdColumn(table.Alias);

        foreach ((string columnName, object? columnValue) in columnsToSet)
        {
            if (columnName == idColumn.Name)
            {
                object? defaultIdValue = columnValue == null ? null : RuntimeTypeConverter.GetDefaultValue(columnValue.GetType());

                if (Equals(columnValue, defaultIdValue))
                {
                    continue;
                }
            }

            ColumnNode column = table.GetColumn(columnName, null, table.Alias);
            ParameterNode parameter = ParameterGenerator.Create(columnValue);

            var assignment = new ColumnAssignmentNode(column, parameter);
            assignments.Add(assignment);
        }

        return assignments.AsReadOnly();
    }
}
