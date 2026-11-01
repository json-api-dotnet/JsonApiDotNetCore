# Resource Definitions

_since v2.3.4_

Resource definitions provide a resource-oriented way to handle custom business logic (irrespective of the originating endpoint).
They are resolved from the dependency injection container, so you can inject dependencies in their constructor.

In v4.2 we introduced an extension method that you can use to register your resource definition.

```c#
// Program.cs
builder.Services.AddResourceDefinition<ArticleDefinition>();
```

> [!TIP]
> If you're using [auto-discovery](~/usage/resource-graph.md#auto-discovery), then resource services, repositories and resource definitions will be automatically registered for you.

> [!NOTE]
> Prior to the introduction of auto-discovery (in v3), you needed to register the resource definition on the container yourself:
> ```c#
> builder.Services.AddScoped<ResourceDefinition<Article, long>, ArticleDefinition>();
> ```

## Customizing queries

_since v4.0_

For various reasons (see examples below) you may need to change parts of the query, depending on resource type.
`JsonApiResourceDefinition<TResource, TId>` (which is an empty implementation of `IResourceDefinition<TResource, TId>`) provides overridable methods that pass you the result of query string parameter parsing.
The value returned by you determines what will be used to execute the query.

An intermediate format (`QueryExpression` and derived types) is used, which enables us to separate JSON:API implementation
from Entity Framework Core `IQueryable` execution.

### Excluding fields

There are some cases where you want attributes or relationships conditionally excluded from your resource response.
For example, you may accept some sensitive data that should only be exposed to administrators after creation.

> [!NOTE]
> To exclude fields unconditionally, [attribute capabilities](~/usage/resources/attributes.md#capabilities) and [relationship capabilities](~/usage/resources/relationships.md#capabilities) can be used instead.

```c#
public class UserDefinition : JsonApiResourceDefinition<User, long>
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

#### Nested fields of compound attributes

_since v6.0_

A sparse fieldset is a tree: elements can have children, which are the selected members of a compound attribute.
The `Including` and `Excluding` extension methods accept a lambda that selects a field, a chain of fields through compound attributes (and to-one relationships),
or multiple of them in an anonymous type:

```c#
public class ContactDefinition : JsonApiResourceDefinition<Contact, long>
{
    public ContactDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override SparseFieldSetExpression? OnApplySparseFieldSet(
        SparseFieldSetExpression? existingSparseFieldSet)
    {
        return existingSparseFieldSet
            .Including<Contact>(contact => contact.Name!.LastName, ResourceGraph)
            .Excluding<Contact>(contact => new
            {
                contact.Notes,
                contact.Name!.SocialSecurityNumber
            }, ResourceGraph);
    }
}
```

The same methods exist with a `ResourceFieldAttribute` or a `ResourceFieldChainExpression` as parameter, which can be used for chains through
compound collections (a lambda cannot select into the elements of a collection).

Some rules to be aware of:

- When the existing fieldset is `null`, which means no restriction, both methods return `null`. For `Excluding`, the excluded field is then still
  fetched from the data store, but it is removed before the response is written.
- Including a nested member of a compound that is already selected as a whole has no effect. Including the compound as a whole when only some of its
  members are selected widens the selection to the whole compound.
- Excluding a nested member of a compound that is selected as a whole replaces the compound by its remaining viewable members. Excluding a compound
  removes all its selected nested members. Members that are not viewable are never added.
- Returning `null` from the hook means that all viewable fields are used.

The hook is called once per resource type during query composition, and once more per resource type while writing the response. In the latter case
the existing fieldset is never `null` (it contains all viewable fields when the client did not specify a fieldset), and the result of the hook is intersected
with it, so the hook cannot add fields there that the client did not select.

### Default sort order

You can define the default sort order if no `sort` query string parameter is provided.

```c#
public class AccountDefinition : JsonApiResourceDefinition<Account, long>
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
public class AccessLogDefinition : JsonApiResourceDefinition<AccessLog, long>
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
public class AccountDefinition : JsonApiResourceDefinition<Account, long>
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
public class EmployeeDefinition : JsonApiResourceDefinition<Employee, long>
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
            throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
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

> [!NOTE]
> This directly influences the Entity Framework Core `IQueryable`. As opposed to using `OnApplyFilter`, this enables the full range of Entity Framework Core operators.
But it only works on primary resource endpoints (for example: /articles, but not on /blogs/1/articles or /blogs?include=articles).

```c#
public class ItemDefinition : JsonApiResourceDefinition<Item, long>
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

### When the query hooks are called

_since v6.0_

The query hooks (`OnApplyPagination`, `OnApplyFilter`, `OnApplySort`, `OnApplySparseFieldSet` and `OnApplyIncludes`) are called per resource type
while the query layers are composed, so also for the resource types of included resources. The table shows the hooks that are
called for the resource type(s) involved in a request, in the order in which they currently occur. This order is an implementation
detail, which can change in future versions.

| Request                                   | Hooks                                                                                                                                               |
|-------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| `GET /stars`                              | Pagination, Filter, Sort, SparseFieldSet, Includes, then (for the response) SparseFieldSet and `GetMeta` for each resource                          |
| `GET /stars?include=planets`              | As above, with Filter, Sort, Pagination, SparseFieldSet and Includes of `Planet` after the Includes of `Star`                                       |
| `GET /stars/1`                            | Filter, SparseFieldSet, Includes, then SparseFieldSet and `GetMeta` for the response                                                                |
| `GET /stars/1/planets`                    | Pagination, Filter, Sort, SparseFieldSet, Includes of `Planet`, Filter of `Star` (twice), Filter of `Planet`, then SparseFieldSet and `GetMeta`     |
| `GET /stars/1/relationships/planets`      | Filter, Sort, Pagination of `Planet`, Filter of `Star` (twice), Filter of `Planet`; SparseFieldSet and Includes are not called                      |
| `POST /stars`                             | Filter, Includes, SparseFieldSet, `GetMeta` (in addition to the write hooks described below)                                                        |
| `PATCH /stars/1`                          | Filter (twice), Includes (in addition to the write hooks)                                                                                           |
| `DELETE /stars/1`                         | None of the query hooks                                                                                                                             |

`OnApplySparseFieldSet` is called once per resource type for composing the query and once per resource type for writing the response, even when
the same type occurs in multiple layers (for example `GET /moons?include=orbitsAround.moons` calls it twice for `Moon` and twice for `Planet`).
Each hook receives the expression produced from the query string (see [query strings](~/usage/extensibility/query-strings.md)), or `null` when absent,
and returns the expression that is used. For `OnApplySparseFieldSet`, the result is additionally intersected with the fieldset from the query string when writing the response.

The hooks work the same for resources that have compound attributes and collections; the selection of nested members is part of the sparse fieldset.

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

## Compound attributes

_since v6.0_

The properties of a [compound type](~/usage/resources/attributes.md#members-without-attr) that do not have `[Attr]` are not visible to the API, but they are part of the resource instance
that resource definitions receive. They can be set from `OnPrepareWriteAsync` and `OnWritingAsync`, for example to stamp a version or timestamp:

```c#
public class ContactDefinition : JsonApiResourceDefinition<Contact, long>
{
    public ContactDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override Task OnWritingAsync(Contact resource,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (resource.Company != null)
        {
            // Not an [Attr], so it never appears in request or response bodies.
            resource.Company.LastUpdatedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}
```

Such members are fetched from the data store before an update, so `OnPrepareWriteAsync` sees the stored values. But because a PATCH
replaces the entire compound value (request bodies cannot contain the non-exposed members), the stored values are lost unless the definition copies them from the
resource it received in `OnPrepareWriteAsync` into the new value in `OnWritingAsync`. Non-exposed members take part in change detection, so
a change made by a definition results in a response body instead of 204 No Content.
