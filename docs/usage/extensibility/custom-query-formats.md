# Custom QueryString parameters

For information on the built-in query string parameters, see the documentation for them.
In order to add parsing of custom query string parameters, you can implement the `IQueryStringParameterReader` interface and inject it.

```c#
public class YourQueryStringParameterReader : IQueryStringParameterReader
{
    // ...
}
```

```c#
services.AddScoped<YourQueryStringParameterReader>();
services.AddScoped<IQueryStringParameterReader>(sp => sp.GetService<YourQueryStringParameterReader>());
```
