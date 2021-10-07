# Resource Definitions

_since v2.3.4_

Resource definitions provide a resource-oriented way to handle custom business logic (irrespective of the originating endpoint).
They are resolved from the dependency injection container, so you can inject dependencies in their constructor.

In v4.2 we introduced an extension method that you can use to register your resource definition.

**Note:** If you're using [auto-discovery](~/usage/resource-graph.md#auto-discovery), this happens automatically.

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceDefinition<ArticleDefinition>();
    }
}
```

**Note:** Prior to the introduction of auto-discovery (in v3), you needed to register the
resource definition on the container yourself:

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

**Note:** to exclude attributes unconditionally, use `[Attr(Capabilities = ~AttrCapabilities.AllowView)]` on a resource class property.

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
        var isSuspendedAttribute = ResourceType.Attributes.Single(account =>
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

    public override IImmutableList<IncludeElementExpression> OnApplyIncludes(
        IImmutableList<IncludeElementExpression> existingIncludes)
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

## Handling resource changes

_since v4.2_

Without going into too much details, the diagrams below demonstrate a few scenarios where custom code interacts with write operations.
Click on a diagram to open it full-size in a new window.

### Create resource

<a href="~/diagrams/resource-definition-create-resource.svg" target="_blank">
<img src="~/diagrams/resource-definition-create-resource.svg" style="border: 1px solid #000">
</a>

1. User sends request to create a resource
2. An empty resource instance is created
3. Developer sets default values for attribute 1 and 2
4. Attribute 1 and 3 from incoming request are copied into default instance
5. Developer overwrites attribute 3
6. Row is inserted in database
7. Developer sends notification to service bus
8. The new resource is fetched
9. Resource is sent back to the user

### Update Resource

<a href="~/diagrams/resource-definition-update-resource.svg" target="_blank">
<img src="~/diagrams/resource-definition-update-resource.svg" style="border: 1px solid #000">
</a>

1. User sends request to update resource with ID 1
2. Existing resource is fetched from database
3. Developer changes attribute 1 and 2
4. Attribute 1 and 3 from incoming request are copied into fetched instance
5. Developer overwrites attribute 3
6. Row is updated in database
7. Developer sends notification to service bus
8. The resource is fetched, along with requested includes
9. Resource with includes is sent back to the user

### Delete Resource

<a href="~/diagrams/resource-definition-delete-resource.svg" target="_blank">
<img src="~/diagrams/resource-definition-delete-resource.svg" style="border: 1px solid #000">
</a>

1. User sends request to delete resource with ID 1
2. Developer runs custom validation logic
3. Row is deleted from database
4. Developer sends notification to service bus
5. Success status is sent back to user

### Set Relationship

<a href="~/diagrams/resource-definition-set-relationship.svg" target="_blank">
<img src="~/diagrams/resource-definition-set-relationship.svg" style="border: 1px solid #000">
</a>

1. User sends request to assign two resources (green) to relationship 'name' (black) on resource 1 (yellow)
2. Existing resource (blue) with related resources (red) is fetched from database
3. Developer changes attributes (not shown in diagram for brevity)
4. Developer removes one resource from the to-be-assigned set (green)
5. Existing resources in relationship (red) are replaced with resource from previous step (green)
6. Developer overwrites attributes (not shown in diagram for brevity)
7. Resource and relationship are updated in database
8. Developer sends notification to service bus
9. Success status is sent back to user
