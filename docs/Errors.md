---
currentMenu: errors
---

# Custom Errors

By default, errors will only contain the properties defined by the internal [Error](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Internal/Error.cs) class. However, you can create your own by inheriting from `Error` and either throwing it in a `JsonApiException` or returning the error from your controller.

```csharp
// custom error definition
public class CustomError : Error {
    public CustomError(string status, string title, string detail, string myProp)
    : base(status, title, detail)
    {
        MyCustomProperty = myProp;
    }
    public string MyCustomProperty { get; set; }
}

// throwing a custom error
public void MyMethod() {
    var error = new CustomError("507", "title", "detail", "custom");
    throw new JsonApiException(error);
}

// returning from controller
[HttpPost]
public override async Task<IActionResult> PostAsync([FromBody] MyEntity entity)
{
    if(_db.IsFull)
        return Error(new CustomError("507", "Database is full.", "Theres no more room.", "Sorry."));

    if(model.Validations.IsValid == false)
        return Errors(model.Validations.Select(v => v.GetErrors()));
        
    // ...
}
```