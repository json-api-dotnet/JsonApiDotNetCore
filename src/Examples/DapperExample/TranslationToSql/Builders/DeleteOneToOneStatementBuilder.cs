using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.Builders;

internal sealed class DeleteOneToOneStatementBuilder(IDataModelService dataModelService) : StatementBuilder(dataModelService)
{
    public DeleteNode Build(ResourceType resourceType, string whereColumnName, object? whereValue)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNullNorEmpty(whereColumnName);

        ResetState();

        TableNode table = GetTable(resourceType, null);

        ColumnNode column = table.GetColumn(whereColumnName, null, table.Alias);
        WhereNode where = GetWhere(column, whereValue);

        return new DeleteNode(table, where);
    }

    private WhereNode GetWhere(ColumnNode column, object? value)
    {
        ParameterNode parameter = ParameterGenerator.Create(value);
        var filter = new ComparisonNode(ComparisonOperator.Equals, column, parameter);
        return new WhereNode(filter);
    }
}
