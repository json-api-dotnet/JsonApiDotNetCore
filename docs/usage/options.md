# Global Options

Configuration can be applied when adding services to the dependency injection container at startup.

```c#
// Program.cs
builder.Services.AddJsonApi<AppDbContext>(options =>
{
    // Configure the options here...
});
```

## Client-generated IDs

By default, the server will respond with a 403 Forbidden HTTP Status Code if a POST request is received with a client-generated ID.

However, this can be allowed or required globally (for all resource types) by setting `ClientIdGeneration` in options:

```c#
options.ClientIdGeneration = ClientIdGenerationMode.Allowed;
```

or:

```c#
options.ClientIdGeneration = ClientIdGenerationMode.Required;
```

It is possible to overrule this setting per resource type:

```c#
[Resource(ClientIdGeneration = ClientIdGenerationMode.Required)]
public class Article : Identifiable<Guid>
{
    // ...
}
```

> [!NOTE]
> JsonApiDotNetCore versions before v5.4.0 only provided the global `AllowClientGeneratedIds` boolean property.

## Pagination

The default page size used for all resources can be overridden in options (10 by default). To disable pagination, set it to `null`.
The maximum page size and number allowed from client requests can be set too (unconstrained by default).

You can also include the total number of resources in each response.

> [!NOTE]
> Including the total number of resources adds some overhead, because the count is fetched in a separate query.

```c#
options.DefaultPageSize = new PageSize(25);
options.MaximumPageSize = new PageSize(100);
options.MaximumPageNumber = new PageNumber(50);
options.IncludeTotalResourceCount = true;
```

To retrieve the total number of resources on secondary and relationship endpoints, the reverse of the relationship must to be available. For example, in `GET /customers/1/orders`, both the relationships `[HasMany] Customer.Orders` and `[HasOne] Order.Customer` must be defined.
If `IncludeTotalResourceCount` is set to `false` (or the inverse relationship is unavailable on a non-primary endpoint), best-effort pagination links are returned instead. This means no `last` link and the `next` link only occurs when the current page is full.

> [!TIP]
> Since v5.8, pagination can be [turned off per relationship](~/usage/resources/relationships.md#disable-pagination).

## Relative Links

All links are absolute by default. However, you can configure relative links:

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
         "self": "/articles/4309/relationships/author",
         "related": "/articles/4309/author"
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

### Naming policies

Two settings play a role, and they have different responsibilities:

- `PropertyNamingPolicy` determines the names that are derived from your model, when no explicit name is specified:
  - resource types (the `type` value and route), after pluralization
  - attributes and relationships
  - _since v6.0_: members of [compound attributes](~/usage/resources/attributes.md#compound-attributes), at every nesting level, including the members of compound collections

  This includes the names used in query strings, such as in `filter`, `sort` and `fields`. An explicit `[Resource(PublicName = "...")]` or `[Attr(PublicName = "...")]` always wins
  and is used verbatim, so it is not converted by the policy, at every nesting level.
  The names are determined once at startup, see [here](~/usage/resources/attributes.md#name).
- `DictionaryKeyPolicy` is meant for keys of dictionaries, such as the keys of `meta` (including the `total` that is added when `IncludeTotalResourceCount` is on),
  and dictionary values in general. It does not apply to names that are derived from your model.

Neither policy affects the names that are defined by the JSON:API specification, such as `data`, `attributes`, `relationships`, `type`, `id`, `errors`, `links` and `meta`.
Function names and keywords in query strings (such as `has`, `count`, `equals`, `any` and the element symbol `_`) are not affected either.

JSON annotations such as `[JsonPropertyName]` and `[JsonIgnore]` on resource and compound types are never taken into account, because everything is driven by `[Attr]`.
Use `[Attr(PublicName = "...")]` to customize a name instead, and the `Capabilities` property (for example `~AttrCapabilities.AllowView`) to hide a member.


## ModelState Validation

[ASP.NET ModelState validation](https://learn.microsoft.com/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api) can be used to validate incoming request bodies when creating and updating resources. Since v5.0, this is enabled by default.
When `ValidateModelState` is set to `false`, no model validation is performed.

How nullability affects ModelState validation is described [here](~/usage/resources/nullability.md).
Validation also applies to members of [compound attributes](~/usage/advanced/model-state.md#compound-and-collection-attributes) and elements of collections.

```c#
options.ValidateModelState = true;
```

```c#
#nullable enable

public class Person : Identifiable<long>
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
