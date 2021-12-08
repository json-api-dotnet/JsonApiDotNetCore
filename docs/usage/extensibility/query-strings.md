# Query string parameters

The parsing of built-in query string parameters is done by types that implement `IQueryStringParameterReader`, which accepts the incoming parameter value.
Those types also implement `IQueryConstraintProvider`, which they use to expose the parse result.

The parse result consists of an expression in a scope. For example:


```
?filter[articles]=lessThan(price,'1.23')
```

has expression `lessThan(price,'1.23')` in scope `articles`.

For more information on the various built-in query string parameters, see the documentation for them.

## Custom query string parameters

When using Entity Framework Core, resource definitions provide a concise syntax to bind a LINQ expression to a query string parameter.
See [here](~/usage/extensibility/resource-definitions.md#custom-query-string-parameters) for details.

## Custom query string parsing

In order to add parsing of custom query string parameters, you can implement the `IQueryStringParameterReader` interface and register your reader.

```c#
public class CustomQueryStringParameterReader : IQueryStringParameterReader
{
    // ...
}
```

```c#
// Program.cs
builder.Services.AddScoped<CustomQueryStringParameterReader>();
builder.Services.AddScoped<IQueryStringParameterReader>(serviceProvider =>
    serviceProvider.GetRequiredService<CustomQueryStringParameterReader>());
```

Now you can inject your custom reader in resource services, repositories, resource definitions etc.
