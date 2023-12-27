using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.Builders;

internal sealed class UpdateResourceStatementBuilder(IDataModelService dataModelService) : StatementBuilder(dataModelService)
{
    public UpdateNode Build(ResourceType resourceType, IReadOnlyDictionary<string, object?> columnsToUpdate, params object[] idValues)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNullNorEmpty(columnsToUpdate);
        ArgumentGuard.NotNullNorEmpty(idValues);

        ResetState();

        TableNode table = GetTable(resourceType, null);
        List<ColumnAssignmentNode> assignments = GetColumnAssignments(columnsToUpdate, table);

        ColumnNode idColumn = table.GetIdColumn(table.Alias);
        WhereNode where = GetWhere(idColumn, idValues);

        return new UpdateNode(table, assignments, where);
    }

    private List<ColumnAssignmentNode> GetColumnAssignments(IReadOnlyDictionary<string, object?> columnsToUpdate, TableNode table)
    {
        List<ColumnAssignmentNode> assignments = [];

        foreach ((string columnName, object? columnValue) in columnsToUpdate)
        {
            ColumnNode column = table.GetColumn(columnName, null, table.Alias);
            ParameterNode parameter = ParameterGenerator.Create(columnValue);

            var assignment = new ColumnAssignmentNode(column, parameter);
            assignments.Add(assignment);
        }

        return assignments;
    }

    private WhereNode GetWhere(ColumnNode idColumn, IEnumerable<object> idValues)
    {
        List<ParameterNode> parameters = idValues.Select(idValue => ParameterGenerator.Create(idValue)).ToList();
        FilterNode filter = parameters.Count == 1 ? new ComparisonNode(ComparisonOperator.Equals, idColumn, parameters[0]) : new InNode(idColumn, parameters);
        return new WhereNode(filter);
    }
}
