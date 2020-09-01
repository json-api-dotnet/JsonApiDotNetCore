# Global Options

Configuration can be applied when adding the services to the DI container.

```c#
public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddJsonApi<AppDbContext>(options =>
        {
            // configure the options here
        });
    }
}
```

## Client Generated IDs

By default, the server will respond with a 403 Forbidden HTTP Status Code if a POST request is received with a client-generated ID.

However, this can be allowed by setting the AllowClientGeneratedIds flag in the options

```c#
options.AllowClientGeneratedIds = true;
```

## Pagination

The default page size used for all resources can be overridden in options (10 by default). To disable paging, set it to `null`.
The maximum page size and number allowed from client requests can be set too (unconstrained by default).
You can also include the total number of resources in each request. Note that when using this feature, it does add some query overhead since we have to also request the total number of resources.

```c#
options.DefaultPageSize = new PageSize(25);
options.MaximumPageSize = new PageSize(100);
options.MaximumPageNumber = new PageNumber(50);
options.IncludeTotalResourceCount = true;
```

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

If you would like to use unknown query string parameters (parameters not reserved by the json:api specification or registered using ResourceDefinitions), you can set `AllowUnknownQueryStringParameters = true`.
When set, an HTTP 400 Bad Request is returned for unknown query string parameters.

```c#
options.AllowUnknownQueryStringParameters = true;
```

## Maximum include depth

To limit the maximum depth of nested includes, use `MaximumIncludeDepth`. This is null by default, which means unconstrained. If set and a request exceeds the limit, an HTTP 400 Bad Request is returned.

```c#
options.MaximumIncludeDepth = 1;
```

## Custom Serializer Settings

We use Newtonsoft.Json for all serialization needs.
If you want to change the default serializer settings, you can:

```c#
options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
options.SerializerSettings.Converters.Add(new StringEnumConverter());
options.SerializerSettings.Formatting = Formatting.Indented;
```

Because we copy resource properties into an intermediate object before serialization, Newtonsoft.Json annotations on properties are ignored.


## Enable ModelState Validation

If you would like to use ASP.NET Core ModelState validation into your controllers when creating / updating resources, set `ValidateModelState = true`. By default, no model validation is performed.

```c#
options.ValidateModelState = true;
```

You will need to use the JsonApiDotNetCore 'IsRequiredAttribute' instead of the built-in 'RequiredAttribute' because it contains modifications to enable partial patching.

```c#
public class Person : Identifiable
{
    [IsRequired(AllowEmptyStrings = true)]
    public string FirstName { get; set; }
}
```
