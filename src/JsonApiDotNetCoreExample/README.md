# Task List 

- [x] Add inbound serialization formatting
- [x] Add POST
- [x] Add PATCH
- [x] Add DELETE
- [ ] Add Relationships
- [ ] Add support for authorization/context-scoping middleware
- [ ] BadRequest should be 422 in POST
- [ ] Add integration tests to example project

## Usage

- Add Middleware
- Define Models
- Define Controllers

## Defining Models

## Defining Controllers

```
```

#### Non-Integer Type Keys

If your model is using a type other than `int` for the primary key,
you should explicitly declare it in the controller generic:

```
[Route("api/[controller]")]
public class ThingsController : JsonApiController<Person, Guid>
{
    public ThingsController(
        ILoggerFactory loggerFactory,
        AppDbContext context, 
        IJsonApiContext jsonApiContext) 
        : base(loggerFactory, context, jsonApiContext)
    { }
}
```