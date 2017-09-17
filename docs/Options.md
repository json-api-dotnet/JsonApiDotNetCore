---
currentMenu: options
---

# Global Options

## Client Generated Ids

By default, the server will respond with a `403 Forbidden` HTTP Status Code if a `POST` request is
received with a client generated id. However, this can be allowed by setting the `AllowClientGeneratedIds`
flag in the options:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.AllowClientGeneratedIds = true);
    // ...
}
```

## Pagination

If you would like pagination implemented by default, you can specify the page size
when setting up the services:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.DefaultPageSize = 10);
    // ...
}
```

### Total Record Count

The total number of records can be added to the document meta by setting it in the options:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(opt =>
    {
        opt.DefaultPageSize = 5;
        opt.IncludeTotalRecordCount = true;
    });
    // ...
}
```

## Relative Links

All links are absolute by default. However, you can configure relative links:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.RelativeLinks = true);
    // ...
}
```


```http
GET /api/v1/articles/4309 HTTP/1.1
Accept: application/vnd.api+json
```

```json
{
    "type": "articles",
    "id": "4309",
    "attributes": {
        "name": "Voluptas iure est molestias."
    },
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