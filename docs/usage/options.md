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

## Client Generated Ids

By default, the server will respond with a 403 Forbidden HTTP Status Code if a POST request is received with a client-generated ID.

However, this can be allowed by setting the AllowClientGeneratedIds flag in the options

```c#
options.AllowClientGeneratedIds = true;
```

## Pagination

The default page size used for all resources can be overridden in options (10 by default). To disable paging, set it to 0.
The maximum page size and maximum page number allowed from client requests can be set too (unconstrained by default).
You can also include the total number of records in each request. Note that when using this feature, it does add some query overhead since we have to also request the total number of records.

```c#
options.DefaultPageSize = 25;
options.MaximumPageSize = 100;
options.MaximumPageNumber = 50;
```

## Relative Links

All links are absolute by default. However, you can configure relative links.

```c#
options.RelativeLinks = true;
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

## Custom Query String Parameters

If you would like to use custom query string parameters (parameters not reserved by the json:api specification), you can set `AllowCustomQueryStringParameters = true`. The default behavior is to return an HTTP 400 Bad Request for unknown query string parameters.

```c#
options.AllowCustomQueryStringParameters = true;
```

## Custom Serializer Settings

We use Newtonsoft.Json for all serialization needs.
If you want to change the default serializer settings, you can:

```c#
options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
options.SerializerSettings.Converters.Add(new StringEnumConverter());
options.SerializerSettings.Formatting = Formatting.Indented;
```

## Enable ModelState Validation

If you would like to use ASP.NET Core ModelState validation into your controllers when creating / updating resources, set `ValidateModelState = true`. By default, no model validation is performed.

```c#
options.ValidateModelState = true;
```

