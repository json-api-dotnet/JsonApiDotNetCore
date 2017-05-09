---
currentMenu: options
---

# Global Options

## Client Generated Ids

By default, the server will respond with a `403 Forbidden` HTTP Status Code if a `POST` request is
received with a client generated id. However, this can be allowed by setting the `AllowClientGeneratedIds`
flag in the options:

```csharp
services.AddJsonApi<AppDbContext>(opt =>
{
    opt.AllowClientGeneratedIds = true;
    // ..
});
```

## Pagination

If you would like pagination implemented by default, you can specify the page size
when setting up the services:

```csharp
 services.AddJsonApi<AppDbContext>(
     opt => opt.DefaultPageSize = 10);
```

### Total Record Count

The total number of records can be added to the document meta by setting it in the options:

```csharp
services.AddJsonApi<AppDbContext>(opt =>
{
    opt.DefaultPageSize = 5;
    opt.IncludeTotalRecordCount = true;
});
```
