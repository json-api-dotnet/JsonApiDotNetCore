# Errors

Errors returned will contain only the properties that are set on the `Error` class. Custom fields can be added through `Error.Meta`.
You can create a custom error by throwing a `JsonApiException` (which accepts an `Error` instance), or returning an `Error` instance from an `ActionResult` in a controller.
Please keep in mind that JSON:API requires Title to be a generic message, while Detail should contain information about the specific problem occurence.

From a controller method:

```c#
return Conflict(new Error(HttpStatusCode.Conflict)
{
    Title = "Target resource was modified by another user.",
    Detail = $"User {userName} changed the {resourceField} field on the {resourceName} resource."
});
```

From other code:

```c#
throw new JsonApiException(new Error(HttpStatusCode.Conflict)
{
    Title = "Target resource was modified by another user.",
    Detail = $"User {userName} changed the {resourceField} field on the {resourceName} resource."
});
```

In both cases, the middleware will properly serialize it and return it as a JSON:API error.

# Exception handling

The translation of user-defined exceptions to error responses can be customized by registering your own handler.
This handler is also the place to choose the log level and message, based on the exception type.

```c#
public class ProductOutOfStockException : Exception
{
    public int ProductId { get; }

    public ProductOutOfStockException(int productId)
    {
        ProductId = productId;
    }
}

public class CustomExceptionHandler : ExceptionHandler
{
    public CustomExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
        : base(loggerFactory, options)
    {
    }

    protected override LogLevel GetLogLevel(Exception exception)
    {
        if (exception is ProductOutOfStockException)
        {
            return LogLevel.Information;
        }

        return base.GetLogLevel(exception);
    }

    protected override string GetLogMessage(Exception exception)
    {
        if (exception is ProductOutOfStockException productOutOfStock)
        {
            return $"Product {productOutOfStock.ProductId} is currently unavailable.";
        }

        return base.GetLogMessage(exception);
    }

    protected override ErrorDocument CreateErrorDocument(Exception exception)
    {
        if (exception is ProductOutOfStockException productOutOfStock)
        {
            return new ErrorDocument(new Error(HttpStatusCode.Conflict)
            {
                Title = "Product is temporarily available.",
                Detail = $"Product {productOutOfStock.ProductId} cannot be ordered at the moment."
            });
        }

        return base.CreateErrorDocument(exception);
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IExceptionHandler, CustomExceptionHandler>();
    }
}
```
