# Global Options

Configuration can be applied when adding services to the dependency injection container at startup.

```c#
// Program.cs
builder.Services.AddJsonApi<AppDbContext>(options =>
{
    // Configure the options here...
});
```

## Client Generated IDs

By default, the server will respond with a 403 Forbidden HTTP Status Code if a POST request is received with a client-generated ID.

However, this can be allowed by setting the AllowClientGeneratedIds flag in the options:

```c#
options.AllowClientGeneratedIds = true;
```

## Pagination

The default page size used for all resources can be overridden in options (10 by default). To disable paging, set it to `null`.
The maximum page size and number allowed from client requests can be set too (unconstrained by default).
You can also include the total number of resources in each response. Note that when using this feature, it does add some query overhead since we have to also request the total number of resources.

```c#
options.DefaultPageSize = new PageSize(25);
options.MaximumPageSize = new PageSize(100);
options.MaximumPageNumber = new PageNumber(50);
options.IncludeTotalResourceCount = true;
```

To retrieve the total number of resources on secondary and relationship endpoints, the reverse of the relationship must to be available. For example, in `GET /customers/1/orders`, both the relationships `[HasMany] Customer.Orders` and `[HasOne] Order.Customer` must be defined.
If `IncludeTotalResourceCount` is set to `false` (or the inverse relationship is unavailable on a non-primary endpoint), best-effort paging links are returned instead. This means no `last` link and the `next` link only occurs when the current page is full.

## Relative Links

All links are absolute by default. However, you can configure relative links.

```c#
options.UseRelativeLinks = true;
```

```json
{
  "type": "articles",
  "id": "4309",
  "relationships": {
     "author": {
       "links": {
         "self": "/api/v1/articles/4309/relationships/author",
         "related": "/api/v1/articles/4309/author"
       }
     }
  }
}
```

## Unknown Query String Parameters

If you would like to allow unknown query string parameters (parameters not reserved by the JSON:API specification or registered using resource definitions), you can set `AllowUnknownQueryStringParameters = true`. When set to `false` (the default), an HTTP 400 Bad Request is returned for unknown query string parameters.

```c#
options.AllowUnknownQueryStringParameters = true;
```

## Maximum include depth

To limit the maximum depth of nested includes, use `MaximumIncludeDepth`. This is null by default, which means unconstrained. If set and a request exceeds the limit, an HTTP 400 Bad Request is returned.

```c#
options.MaximumIncludeDepth = 1;
```

## Customize Serializer options

We use [System.Text.Json](https://www.nuget.org/packages/System.Text.Json) for all serialization needs.
If you want to change the default serializer options, you can:

```c#
options.SerializerOptions.WriteIndented = true;
options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
```

The default naming convention (as used in the routes and resource/attribute/relationship names) is also determined here, and can be changed (default is camel-case):

```c#
// Use Pascal case
options.SerializerOptions.PropertyNamingPolicy = null;
options.SerializerOptions.DictionaryKeyPolicy = null;
```

Because we copy resource properties into an intermediate object before serialization, JSON annotations such as `[JsonPropertyName]` and `[JsonIgnore]` on `[Attr]` properties are ignored.


## ModelState Validation

[ASP.NET ModelState validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation) can be used to validate incoming request bodies when creating and updating resources. Since v5.0, this is enabled by default.
When `ValidateModelState` is set to `false`, no model validation is performed.

How nullability affects ModelState validation is described [here](~/usage/resources/nullability.md).

```c#
options.ValidateModelState = true;
```

```c#
#nullable enable

public class Person : Identifiable<int>
{
    [Attr]
    [MinLength(3)]
    public string FirstName { get; set; } = null!;

    [Attr]
    [Required]
    public int? Age { get; set; }

    [HasOne]
    public LoginAccount Account { get; set; } = null!;
}
```
