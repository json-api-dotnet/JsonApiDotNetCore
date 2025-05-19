using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

internal sealed class SumWhereClauseBuilder : WhereClauseBuilder
{
    public override Expression DefaultVisit(QueryExpression expression, QueryClauseBuilderContext context)
    {
        if (expression is SumExpression sumExpression)
        {
            return VisitSum(sumExpression, context);
        }

        return base.DefaultVisit(expression, context);
    }

    private MethodCallExpression VisitSum(SumExpression expression, QueryClauseBuilderContext context)
    {
        Expression collectionPropertyAccess = Visit(expression.TargetToManyRelationship, context);

        // TODO: Allow collection attribute.
        ResourceType selectorResourceType = ((HasManyAttribute)expression.TargetToManyRelationship.Fields.Single()).RightType;
        using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(selectorResourceType.ClrType);

        var nestedContext = new QueryClauseBuilderContext(collectionPropertyAccess, selectorResourceType, typeof(Enumerable), context.EntityModel,
            context.LambdaScopeFactory, lambdaScope, context.QueryableBuilder, context.State);

        LambdaExpression lambda = GetSelectorLambda(expression.Selector, nestedContext);

        return SumExtensionMethodCall(lambda, nestedContext);
    }

    private LambdaExpression GetSelectorLambda(QueryExpression expression, QueryClauseBuilderContext context)
    {
        Expression body = Visit(expression, context);
        return Expression.Lambda(body, context.LambdaScope.Parameter);
    }

    private static MethodCallExpression SumExtensionMethodCall(LambdaExpression selector, QueryClauseBuilderContext context)
    {
        return Expression.Call(context.ExtensionType, "Sum", [context.LambdaScope.Parameter.Type], context.Source, selector);
    }
}
