# Errors

By default, errors will only contain the properties defined by the `Error` class. 
However, you can create your own by inheriting from Error and either throwing it in a `JsonApiException` or returning the error from your controller.

```c#
public class CustomError : Error 
{
    public CustomError(int status, string title, string detail, string myProp)
        : base(status, title, detail)
    {
        MyCustomProperty = myProp;
    }

    public string MyCustomProperty { get; set; }
}
```

If you throw a `JsonApiException` that is unhandled, the middleware will properly serialize it and return it as a json:api error.

```c#
public void MyMethod() 
{
    var error = new CustomError(507, "title", "detail", "custom");
    throw new JsonApiException(error);
}
```

You can use the `IActionResult Error(Error error)` method to return a single error message, or you can use the `IActionResult Errors(ErrorCollection errors)` method to return a collection of errors from your controller.

```c#
[HttpPost]
public override async Task<IActionResult> PostAsync([FromBody] MyEntity entity)
{
    if(_db.IsFull)
        return Error(new CustomError("507", "Database is full.", "Theres no more room.", "Sorry."));
            
    if(model.Validations.IsValid == false)
        return Errors(model.Validations.GetErrors());
}
```

## Example: Including Links

This example demonstrates one way you can include links with your error payloads.

This example assumes that there is a support documentation site that provides additional information based on the HTTP Status Code.

```c#
public class LinkableError : Error 
{
    public LinkableError(int status, string title)
        : base(status, title)
    { }

    public ErrorLink Links => "https://example.com/errors/" + Status;
}

var error = new LinkableError(401, "You're not allowed to do that.");
throw new JsonApiException(error);
```






