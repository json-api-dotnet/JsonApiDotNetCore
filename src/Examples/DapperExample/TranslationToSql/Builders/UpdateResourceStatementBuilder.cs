using System.Collections.ObjectModel;
using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.Builders;

internal sealed class UpdateResourceStatementBuilder(IDataModelService dataModelService)
    : StatementBuilder(dataModelService)
{
    public UpdateNode Build(ResourceType resourceType, IReadOnlyDictionary<string, object?> columnsToUpdate, params object[] idValues)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentGuard.NotNullNorEmpty(columnsToUpdate);
        ArgumentGuard.NotNullNorEmpty(idValues);

        ResetState();

        TableNode table = GetTable(resourceType, null);
        ReadOnlyCollection<ColumnAssignmentNode> assignments = GetColumnAssignments(columnsToUpdate, table);

        ColumnNode idColumn = table.GetIdColumn(table.Alias);
        WhereNode where = GetWhere(idColumn, idValues);

        return new UpdateNode(table, assignments, where);
    }

    private ReadOnlyCollection<ColumnAssignmentNode> GetColumnAssignments(IReadOnlyDictionary<string, object?> columnsToUpdate, TableNode table)
    {
        List<ColumnAssignmentNode> assignments = [];

        foreach ((string columnName, object? columnValue) in columnsToUpdate)
        {
            ColumnNode column = table.GetColumn(columnName, null, table.Alias);
            ParameterNode parameter = ParameterGenerator.Create(columnValue);

            var assignment = new ColumnAssignmentNode(column, parameter);
            assignments.Add(assignment);
        }

        return assignments.AsReadOnly();
    }

    private WhereNode GetWhere(ColumnNode idColumn, IEnumerable<object> idValues)
    {
        ReadOnlyCollection<ParameterNode> parameters = idValues.Select(ParameterGenerator.Create).ToArray().AsReadOnly();
        FilterNode filter = parameters.Count == 1 ? new ComparisonNode(ComparisonOperator.Equals, idColumn, parameters[0]) : new InNode(idColumn, parameters);
        return new WhereNode(filter);
    }
}
