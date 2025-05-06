# Public API surface

This topic documents the public API, which is generated from the triple-slash XML documentation comments in source code.
Commonly used types are listed in the following sections.

## Setup

- <xref:JsonApiDotNetCore.Configuration.JsonApiOptions> implements <xref:JsonApiDotNetCore.Configuration.IJsonApiOptions>
- <xref:JsonApiDotNetCore.Configuration.ResourceGraph> implements <xref:JsonApiDotNetCore.Configuration.IResourceGraph>
  - <xref:JsonApiDotNetCore.Configuration.ResourceType>
  - <xref:JsonApiDotNetCore.Resources.Identifiable`1> implements <xref:JsonApiDotNetCore.Resources.IIdentifiable`1>
    - <xref:JsonApiDotNetCore.Resources.Annotations.ResourceAttribute> and <xref:JsonApiDotNetCore.Resources.Annotations.NoResourceAttribute>
    - <xref:JsonApiDotNetCore.Resources.Annotations.ResourceLinksAttribute>
    - <xref:JsonApiDotNetCore.Resources.Annotations.AttrAttribute>
    - <xref:JsonApiDotNetCore.Resources.Annotations.HasOneAttribute>
    - <xref:JsonApiDotNetCore.Resources.Annotations.HasManyAttribute>
    - <xref:JsonApiDotNetCore.Resources.Annotations.EagerLoadAttribute>
- <xref:JsonApiDotNetCore.Configuration.ServiceCollectionExtensions>, <xref:JsonApiDotNetCore.OpenApi.Swashbuckle.ServiceCollectionExtensions> (OpenAPI)
- <xref:JsonApiDotNetCore.Configuration.ApplicationBuilderExtensions>
- <xref:JsonApiDotNetCore.Middleware.JsonApiRoutingConvention> implements <xref:JsonApiDotNetCore.Middleware.IJsonApiRoutingConvention>
  - <xref:JsonApiDotNetCore.Controllers.Annotations.DisableRoutingConventionAttribute>
  - <xref:JsonApiDotNetCore.Controllers.Annotations.DisableQueryStringAttribute>

## Query strings

- <xref:JsonApiDotNetCore.Middleware.AsyncQueryStringActionFilter> implements <xref:JsonApiDotNetCore.Middleware.IAsyncQueryStringActionFilter>
  - <xref:JsonApiDotNetCore.QueryStrings.QueryStringReader> implements <xref:JsonApiDotNetCore.QueryStrings.IQueryStringReader>
    - <xref:JsonApiDotNetCore.QueryStrings.IQueryStringParameterReader> and <xref:JsonApiDotNetCore.Queries.IQueryConstraintProvider>
      - <xref:JsonApiDotNetCore.QueryStrings.IncludeQueryStringParameterReader> implements <xref:JsonApiDotNetCore.QueryStrings.IIncludeQueryStringParameterReader>
        - <xref:JsonApiDotNetCore.Queries.Parsing.IncludeParser> implements <xref:JsonApiDotNetCore.Queries.Parsing.IIncludeParser>
      - <xref:JsonApiDotNetCore.QueryStrings.FilterQueryStringParameterReader> implements <xref:JsonApiDotNetCore.QueryStrings.IFilterQueryStringParameterReader>
        - <xref:JsonApiDotNetCore.Queries.Parsing.FilterParser> implements <xref:JsonApiDotNetCore.Queries.Parsing.IFilterParser>
      - <xref:JsonApiDotNetCore.QueryStrings.SortQueryStringParameterReader> implements <xref:JsonApiDotNetCore.QueryStrings.ISortQueryStringParameterReader>
        - <xref:JsonApiDotNetCore.Queries.Parsing.SortParser> implements <xref:JsonApiDotNetCore.Queries.Parsing.ISortParser>
      - <xref:JsonApiDotNetCore.QueryStrings.PaginationQueryStringParameterReader> implements <xref:JsonApiDotNetCore.QueryStrings.IPaginationQueryStringParameterReader>
        - <xref:JsonApiDotNetCore.Queries.Parsing.PaginationParser> implements <xref:JsonApiDotNetCore.Queries.Parsing.IPaginationParser>
      - <xref:JsonApiDotNetCore.QueryStrings.SparseFieldSetQueryStringParameterReader> implements <xref:JsonApiDotNetCore.QueryStrings.ISparseFieldSetQueryStringParameterReader>
        - <xref:JsonApiDotNetCore.Queries.Parsing.SparseFieldSetParser> implements <xref:JsonApiDotNetCore.Queries.Parsing.ISparseFieldSetParser>
- <xref:JsonApiDotNetCore.Queries.QueryLayer>
  - <xref:JsonApiDotNetCore.Queries.FieldSelection>
  - <xref:JsonApiDotNetCore.Queries.Expressions.QueryExpression>
    - <xref:JsonApiDotNetCore.Queries.Expressions.IncludeExpression>
    - <xref:JsonApiDotNetCore.Queries.Expressions.FilterExpression>
    - <xref:JsonApiDotNetCore.Queries.Expressions.SortExpression>
    - <xref:JsonApiDotNetCore.Queries.Expressions.PaginationExpression>
    - <xref:JsonApiDotNetCore.Queries.Expressions.SparseFieldSetExpression>
- <xref:JsonApiDotNetCore.Queries.QueryableBuilding.QueryableBuilder> implements <xref:JsonApiDotNetCore.Queries.QueryableBuilding.IQueryableBuilder>
  - <xref:JsonApiDotNetCore.Queries.QueryableBuilding.IncludeClauseBuilder> implements <xref:JsonApiDotNetCore.Queries.QueryableBuilding.IIncludeClauseBuilder>
  - <xref:JsonApiDotNetCore.Queries.QueryableBuilding.WhereClauseBuilder> implements <xref:JsonApiDotNetCore.Queries.QueryableBuilding.IWhereClauseBuilder>
  - <xref:JsonApiDotNetCore.Queries.QueryableBuilding.OrderClauseBuilder> implements <xref:JsonApiDotNetCore.Queries.QueryableBuilding.IOrderClauseBuilder>
  - <xref:JsonApiDotNetCore.Queries.QueryableBuilding.SkipTakeClauseBuilder> implements <xref:JsonApiDotNetCore.Queries.QueryableBuilding.ISkipTakeClauseBuilder>
  - <xref:JsonApiDotNetCore.Queries.QueryableBuilding.SelectClauseBuilder> implements <xref:JsonApiDotNetCore.Queries.QueryableBuilding.ISelectClauseBuilder>

## Request pipeline

- <xref:JsonApiDotNetCore.Controllers.JsonApiController`2> implements <xref:JsonApiDotNetCore.Controllers.BaseJsonApiController`2>
  - <xref:JsonApiDotNetCore.Controllers.JsonApiQueryController`2>
  - <xref:JsonApiDotNetCore.Controllers.JsonApiCommandController`2>
- <xref:JsonApiDotNetCore.Controllers.JsonApiOperationsController> implements <xref:JsonApiDotNetCore.Controllers.BaseJsonApiOperationsController>
  - <xref:JsonApiDotNetCore.AtomicOperations.OperationsProcessor> implements <xref:JsonApiDotNetCore.AtomicOperations.IOperationsProcessor>
    - <xref:JsonApiDotNetCore.AtomicOperations.Processors.IOperationProcessor>
      - <xref:JsonApiDotNetCore.AtomicOperations.Processors.CreateProcessor`2> implements <xref:JsonApiDotNetCore.AtomicOperations.Processors.ICreateProcessor`2>
      - <xref:JsonApiDotNetCore.AtomicOperations.Processors.UpdateProcessor`2> implements <xref:JsonApiDotNetCore.AtomicOperations.Processors.IUpdateProcessor`2>
      - <xref:JsonApiDotNetCore.AtomicOperations.Processors.DeleteProcessor`2> implements <xref:JsonApiDotNetCore.AtomicOperations.Processors.IDeleteProcessor`2>
      - <xref:JsonApiDotNetCore.AtomicOperations.Processors.SetRelationshipProcessor`2> implements <xref:JsonApiDotNetCore.AtomicOperations.Processors.ISetRelationshipProcessor`2>
      - <xref:JsonApiDotNetCore.AtomicOperations.Processors.AddToRelationshipProcessor`2> implements <xref:JsonApiDotNetCore.AtomicOperations.Processors.IAddToRelationshipProcessor`2>
      - <xref:JsonApiDotNetCore.AtomicOperations.Processors.RemoveFromRelationshipProcessor`2> implements <xref:JsonApiDotNetCore.AtomicOperations.Processors.IRemoveFromRelationshipProcessor`2>
- <xref:JsonApiDotNetCore.Middleware.JsonApiMiddleware>
  - <xref:JsonApiDotNetCore.Middleware.JsonApiRequest> implements <xref:JsonApiDotNetCore.Middleware.IJsonApiRequest>
- <xref:JsonApiDotNetCore.Services.JsonApiResourceService`2> implements <xref:JsonApiDotNetCore.Services.IResourceService`2>
- <xref:JsonApiDotNetCore.Queries.QueryLayerComposer> implements <xref:JsonApiDotNetCore.Queries.IQueryLayerComposer>
  - <xref:JsonApiDotNetCore.Resources.JsonApiResourceDefinition`2> implements <xref:JsonApiDotNetCore.Resources.IResourceDefinition`2>
- <xref:JsonApiDotNetCore.Repositories.EntityFrameworkCoreRepository`2> implements <xref:JsonApiDotNetCore.Repositories.IResourceRepository`2>
  - <xref:JsonApiDotNetCore.Repositories.IResourceReadRepository`2>
  - <xref:JsonApiDotNetCore.Repositories.IResourceWriteRepository`2>

## Serialization

- <xref:JsonApiDotNetCore.Middleware.JsonApiInputFormatter> implements <xref:JsonApiDotNetCore.Middleware.IJsonApiInputFormatter>
  - <xref:JsonApiDotNetCore.Serialization.Request.JsonApiReader> implements <xref:JsonApiDotNetCore.Serialization.Request.IJsonApiReader>
    - <xref:JsonApiDotNetCore.Serialization.Request.Adapters.DocumentAdapter> implements <xref:JsonApiDotNetCore.Serialization.Request.Adapters.IDocumentAdapter>
      - <xref:JsonApiDotNetCore.Resources.TargetedFields> implements <xref:JsonApiDotNetCore.Resources.ITargetedFields>
- <xref:JsonApiDotNetCore.Middleware.JsonApiOutputFormatter> implements <xref:JsonApiDotNetCore.Middleware.IJsonApiOutputFormatter>
  - <xref:JsonApiDotNetCore.Serialization.Response.JsonApiWriter> implements <xref:JsonApiDotNetCore.Serialization.Response.IJsonApiWriter>
    - <xref:JsonApiDotNetCore.Serialization.Response.ResponseModelAdapter> implements <xref:JsonApiDotNetCore.Serialization.Response.IResponseModelAdapter>
- <xref:JsonApiDotNetCore.Serialization.Objects.Document>
- <xref:JsonApiDotNetCore.Queries.IEvaluatedIncludeCache>
- <xref:JsonApiDotNetCore.Queries.SparseFieldSetCache> implements <xref:JsonApiDotNetCore.Queries.ISparseFieldSetCache>

## Error handling

- <xref:JsonApiDotNetCore.Middleware.AsyncJsonApiExceptionFilter> implements <xref:JsonApiDotNetCore.Middleware.IAsyncJsonApiExceptionFilter>
  - <xref:JsonApiDotNetCore.Middleware.ExceptionHandler> implements <xref:JsonApiDotNetCore.Middleware.IExceptionHandler>
