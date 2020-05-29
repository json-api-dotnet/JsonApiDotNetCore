# Resource Definitions

In order to improve the developer experience, we have introduced a type that makes
common modifications to the default API behavior easier. `ResourceDefinition` was first introduced in v2.3.4.

## Runtime Attribute Filtering

_since v2.3.4_

There are some cases where you want attributes excluded from your resource response.
For example, you may accept some form data that shouldn't be exposed after creation.
This kind of data may get hashed in the database and should never be exposed to the client.

Using the techniques described below, you can achieve the following request/response behavior:

```http
POST /users HTTP/1.1
Content-Type: application/vnd.api+json

{
  "data": {
    "type": "users",
    "attributes": {
      "account-number": "1234567890",
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

### Single Attribute

```c#
public class UserDefinition : ResourceDefinition<User>
{
    public UserDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
    {
        HideFields(user => user.AccountNumber);
    }
}
```

### Multiple Attributes

```c#
public class UserDefinition : ResourceDefinition<User>
{
    public UserDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
    {
        HideFields(user => new {user.AccountNumber, user.Password});
    }
}
```

## Default Sort

_since v3.0.0_

You can define the default sort behavior if no `sort` query is provided.

```c#
public class AccountDefinition : ResourceDefinition<Account>
{
    public override PropertySortOrder GetDefaultSortOrder()
    {
        return new PropertySortOrder
        {
            (account => account.LastLoginTime, SortDirection.Descending),
            (account => account.UserName, SortDirection.Ascending)
        };
    }
}
```

## Custom Query Filters

_since v3.0.0_

You can define additional query string parameters and the query that should be used.
If the key is present in a filter request, the supplied query will be used rather than the default behavior.

```c#
public class ItemDefinition : ResourceDefinition<Item>
{
    // handles queries like: ?filter[was-active-on]=2018-10-15T01:25:52Z
    public override QueryFilters GetQueryFilters()
    {
        return new QueryFilters
        {
            {
                "was-active-on", (items, filter) =>
                {
                    return DateTime.TryParse(filter.Value, out DateTime timeValue)
                        ? items.Where(item => item.ExpireTime == null || timeValue < item.ExpireTime)
                        : throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                        {
                            Title = "Invalid filter value",
                            Detail = $"'{filter.Value}' is not a valid date."
                        });
                }
            }
        };
    }
}
```

## Using ResourceDefinitions Prior to v3

Prior to the introduction of auto-discovery, you needed to register the
`ResourceDefinition` on the container yourself:

```c#
services.AddScoped<ResourceDefinition<Item>, ItemResource>();
```
