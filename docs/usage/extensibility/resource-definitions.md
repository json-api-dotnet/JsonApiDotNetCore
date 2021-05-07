# Resource Definitions

_since v2.3.4_

Resource definitions provide a resource-oriented way to handle custom business logic (irrespective of the originating endpoint).

They are resolved from the dependency injection container, so you can inject dependencies in their constructor.

**Note:** Prior to the introduction of auto-discovery (in v3), you needed to register the
`ResourceDefinition` on the container yourself:

```c#
services.AddScoped<ResourceDefinition<Product>, ProductResource>();
```

## Customizing queries

_since v4.0_

For various reasons (see examples below) you may need to change parts of the query, depending on resource type.
`JsonApiResourceDefinition<TResource>` (which is an empty implementation of `IResourceDefinition<TResource>`) provides overridable methods that pass you the result of query string parameter parsing.
The value returned by you determines what will be used to execute the query.

An intermediate format (`QueryExpression` and derived types) is used, which enables us to separate JSON:API implementation 
from Entity Framework Core `IQueryable` execution.

### Excluding fields

There are some cases where you want attributes (or relationships) conditionally excluded from your resource response.
For example, you may accept some sensitive data that should only be exposed to administrators after creation.

Note: to exclude attributes unconditionally, use `[Attr(Capabilities = ~AttrCapabilities.AllowView)]` on a resource class property.

```c#
public class UserDefinition : JsonApiResourceDefinition<User>
{
    public UserDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override SparseFieldSetExpression OnApplySparseFieldSet(
        SparseFieldSetExpression existingSparseFieldSet)
    {
        if (IsAdministrator)
        {
            return existingSparseFieldSet;
        }

        return existingSparseFieldSet.Excluding<User>(
            user => user.Password, ResourceGraph);
    }
}
```

Using this technique, you can achieve the following request/response behavior:

```http
POST /users HTTP/1.1
Content-Type: application/vnd.api+json

{
  "data": {
    "type": "users",
    "attributes": {
      "password": "secret",
      "name": "John Doe"
    }
  }
}
```

```http
HTTP/1.1 201 Created
Location: http://example.com/users/1
Content-Type: application/vnd.api+json

{
  "data": {
    "type": "users",
    "id": "1",
    "attributes": {
      "name": "John Doe"
    }
  }
}
```

### Default sort order

You can define the default sort order if no `sort` query string parameter is provided.

```c#
public class AccountDefinition : JsonApiResourceDefinition<Account>
{
    public AccountDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override SortExpression OnApplySort(SortExpression existingSort)
    {
        if (existingSort != null)
        {
            return existingSort;
        }

        return CreateSortExpressionFromLambda(new PropertySortOrder
        {
            (account => account.Name, ListSortDirection.Ascending),
            (account => account.ModifiedAt, ListSortDirection.Descending)
        });
    }
}
```

### Enforce page size

You may want to enforce pagination on large database tables.

```c#
public class AccessLogDefinition : JsonApiResourceDefinition<AccessLog>
{
    public AccessLogDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override PaginationExpression OnApplyPagination(
        PaginationExpression existingPagination)
    {
        var maxPageSize = new PageSize(10);

        if (existingPagination != null)
        {
            var pageSize = existingPagination.PageSize?.Value <= maxPageSize.Value
                ? existingPagination.PageSize
                : maxPageSize;

            return new PaginationExpression(existingPagination.PageNumber, pageSize);
        }

        return new PaginationExpression(PageNumber.ValueOne, maxPageSize);
    }
}
```

### Change filters

The next example filters out `Account` resources that are suspended.

```c#
public class AccountDefinition : JsonApiResourceDefinition<Account>
{
    public AccountDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
    {
        var resourceContext = ResourceGraph.GetResourceContext<Account>();

        var isSuspendedAttribute =
            resourceContext.Attributes.Single(account =>
                account.Property.Name == nameof(Account.IsSuspended));

        var isNotSuspended = new ComparisonExpression(ComparisonOperator.Equals,
            new ResourceFieldChainExpression(isSuspendedAttribute),
            new LiteralConstantExpression(bool.FalseString));

        return existingFilter == null
            ? (FilterExpression) isNotSuspended
            : new LogicalExpression(LogicalOperator.And,
                new[] { isNotSuspended, existingFilter });
    }
}
```

### Block including related resources

In the example below, an error is returned when a user tries to include the manager of an employee.

```c#
public class EmployeeDefinition : JsonApiResourceDefinition<Employee>
{
    public EmployeeDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(
        IReadOnlyCollection<IncludeElementExpression> existingIncludes)
    {
        if (existingIncludes.Any(include =>
            include.Relationship.Property.Name == nameof(Employee.Manager)))
        {
            throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
            {
                Title = "Including the manager of employees is not permitted."
            });
        }

        return existingIncludes;
    }
}
```

### Custom query string parameters

_since v3_

You can define additional query string parameters with the LINQ expression that should be used.
If the key is present in a query string, the supplied LINQ expression will be added to the database query.

Note this directly influences the Entity Framework Core `IQueryable`. As opposed to using `OnApplyFilter`, this enables the full range of EF Core operators. 
But it only works on primary resource endpoints (for example: /articles, but not on /blogs/1/articles or /blogs?include=articles).

```c#
public class ItemDefinition : JsonApiResourceDefinition<Item>
{
    public ItemDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override QueryStringParameterHandlers<Item>
        OnRegisterQueryableHandlersForQueryStringParameters()
    {
        return new QueryStringParameterHandlers<Item>
        {
            ["isActive"] = (source, parameterValue) => source
                .Include(item => item.Children)
                .Where(item => item.LastUpdateTime > DateTime.Now.AddMonths(-1)),
            ["isHighRisk"] = FilterByHighRisk
        };
    }

    private static IQueryable<Item> FilterByHighRisk(IQueryable<Item> source,
        StringValues parameterValue)
    {
        bool isFilterOnHighRisk = bool.Parse(parameterValue);

        return isFilterOnHighRisk
            ? source.Where(item => item.RiskLevel >= 5)
            : source.Where(item => item.RiskLevel < 5);
    }
}
```
