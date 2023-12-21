using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.Builders;

internal sealed class UpdateClearOneToOneStatementBuilder(IDataModelService dataModelService) : StatementBuilder(dataModelService)
{
    public UpdateNode Build(ResourceType resourceType, string setColumnName, string whereColumnName, object? whereValue)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(setColumnName);
        ArgumentGuard.NotNull(whereColumnName);

        ResetState();

        TableNode table = GetTable(resourceType, null);

        ColumnNode setColumn = table.GetColumn(setColumnName, null, table.Alias);
        ColumnAssignmentNode columnAssignment = GetColumnAssignment(setColumn);

        ColumnNode whereColumn = table.GetColumn(whereColumnName, null, table.Alias);
        WhereNode where = GetWhere(whereColumn, whereValue);

        return new UpdateNode(table, [columnAssignment], where);
    }

    private WhereNode GetWhere(ColumnNode column, object? value)
    {
        ParameterNode whereParameter = ParameterGenerator.Create(value);
        var filter = new ComparisonNode(ComparisonOperator.Equals, column, whereParameter);
        return new WhereNode(filter);
    }

    private ColumnAssignmentNode GetColumnAssignment(ColumnNode setColumn)
    {
        ParameterNode parameter = ParameterGenerator.Create(null);
        return new ColumnAssignmentNode(setColumn, parameter);
    }
}
