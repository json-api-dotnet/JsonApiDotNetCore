# Task List 

- [x] Add inbound serialization formatting
- [x] Add POST
- [x] Add PATCH
- [x] Add DELETE
- [x] Add Relationships
- [x] Add support for authorization/context-scoping middleware
- [x] Filtering
- [x] Sorting
- [ ] Creating relationships
- [ ] Include Entities
- [ ] BadRequest should be 422 in POST
- [ ] Add integration tests to example project
- [ ] Add logging
- [ ] Configure Different Repositories
- [ ] Contributing Guide
- [ ] Build definitions
- [ ] Tests
- [ ] Allow for presenters with mappings
- [ ] Ability to disable dasherization of names
- [ ] Dasherized route support
- [ ] Sorting/Filtering should be handled by the repository so that it is not dependeny on EF ?
- [ ] Rename ContextEntity ?? 

## Usage

- Add Middleware
- Define Models
- Define Controllers

## Defining Models

Your models should inherit `Identifiable<TId>` where `TId` is the type of the primary key, like so:

```
public class Person : Identifiable<Guid>
{
    public override Guid Id { get; set; }
}
```

### Specifying Public Attributes

If you want an attribute on your model to be publicly available, 
add the `AttrAttribute` and provide the outbound name.

```
public class Person : Identifiable<int>
{
    public override int Id { get; set; }
    
    [Attr("first-name")]
    public string FirstName { get; set; }
}
```

### Relationships

In order for navigation properties to be identified in the model, 
they should be labeled as virtual:

```
public class Person : Identifiable<int>
{
    public override int Id { get; set; }
    
    [Attr("firstName")]
    public string FirstName { get; set; }

    public virtual List<TodoItem> TodoItems { get; set; }
}
```

## Defining Controllers

You need to create controllers that inherit from `JsonApiController<TEntity>` or `JsonApiController<TEntity, TId>`
where `TEntity` is the model that inherits from `Identifiable<TId>`.

```
[Route("api/[controller]")]
public class ThingsController : JsonApiController<Thing>
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IEntityRepository<Thing> entityRepository,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, entityRepository, loggerFactory)
    { }
}
```

#### Non-Integer Type Keys

If your model is using a type other than `int` for the primary key,
you should explicitly declare it in the controller
and repository generic type definitions:

```
[Route("api/[controller]")]
public class ThingsController : JsonApiController<Thing, Guid>
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IEntityRepository<Thing, Guid> entityRepository,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, entityRepository, loggerFactory)
    { }
}
```