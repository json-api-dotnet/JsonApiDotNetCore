# Resource Definitions

In order to improve the developer experience, we have introduced a type that makes
common modifications to the default API behavior easier. `ResourceDefinition` was first introduced in v2.3.4.

## Runtime Attribute Filtering

_since v2.3.4_

There are some cases where you want attributes excluded from your resource response.
For example, you may accept some form data that shouldn't be exposed after creation.
This kind of data may get hashed in the database and should never be exposed to the client.

Using the techniques described below, you can achieve the following reques/response behavior:

```http
POST /users HTTP/1.1
Content-Type: application/vnd.api+json
Accept: application/vnd.api+json

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
public class ModelResource : ResourceDefinition<Model>
{
    protected override List<AttrAttribute> OutputAttrs()
        => Remove(m => m.AccountNumber);
}
```

### Multiple Attributes

```c#
public class ModelResource : ResourceDefinition<Model>
{
    protected override List<AttrAttribute> OutputAttrs()
        => Remove(m => new { m.AccountNumber, m.Password });
}
```

### Derived ResourceDefinitions

If you want to inherit from a different `ResourceDefinition`, these attributes can be composed like so:

```c#
public class BaseResource : ResourceDefinition<Model>
{
    protected override List<AttrAttribute> OutputAttrs()
        => Remove(m => m.TenantId);
}

public class AccountResource : ResourceDefinition<Account>
{
    protected override List<AttrAttribute> OutputAttrs()
        => Remove(m => m.AccountNumber, from: base.OutputAttrs());
}
```

## Default Sort

_since v3.0.0_

You can define the default sort behavior if no `sort` query is provided.

```c#
public class AccountResource : ResourceDefinition<Account>
{
    protected override PropertySortOrder GetDefaultSortOrder()
        => new PropertySortOrder {
            (t => t.Prop, SortDirection.Ascending),
            (t => t.Prop2, SortDirection.Descending),
        };
}
```

## Custom Query Filters

_since v3.0.0_

You can define additional query parameters and the query that should be used.
If the key is present in a filter request, the supplied query will be used rather than the default behavior.

```c#
public class ItemResource : ResourceDefinition<Item>
{
    // handles queries like: ?filter[was-active-on]=2018-10-15T01:25:52Z
    public override QueryFilters GetQueryFilters()
        => new QueryFilters {            
            { "was-active-on", (items, value) => DateTime.TryParse(value, out dateValue)
                ? items.Where(i => i.Expired == null || dateValue < i.Expired)
                : throw new JsonApiException(400, $"'{value}' is not a valid date.")
            }
        };
}
```

## Using ResourceDefinitions Prior to v3

Prior to the introduction of auto-discovery, you needed to register the
`ResourceDefinition` on the container yourself:

```c#
services.AddScoped<ResourceDefinition<Item>, ItemResource>();
```