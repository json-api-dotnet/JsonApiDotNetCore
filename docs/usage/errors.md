# Errors

Errors returned will contain only the properties that are set on the `Error` class. Custom fields can be added through `Error.Meta`.
You can create a custom error by throwing a `JsonApiException` (which accepts an `Error` instance), or returning an `Error` instance from an `ActionResult` in a controller.
Please keep in mind that json:api requires Title to be a generic message, while Detail should contain information about the specific problem occurence.

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

In both cases, the middleware will properly serialize it and return it as a json:api error.
