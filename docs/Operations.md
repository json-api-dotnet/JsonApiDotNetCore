---
currentMenu: operations
---

# Operations

Operations is currently an unofficial proposal. It allows you to perform bulk operations in a single transaction. 

### Enabling

To enable the operations extension, modify you `Startup.ConfigureServices` method:

```csharp
services.AddJsonApi<AppDbContext>(opt => opt.EnableExtension(JsonApiExtension.Operations));
```

### Controllers

To create a bulk operations controller, inherit `JsonApiOperationsController`:

```csharp
[Route("api/bulk")]
public class OperationsController : JsonApiOperationsController
{
    public OperationsController(IOperationsProcessor processor)
        : base(processor)
    { }
}
```

### Example

There is a working example in the `/src/examples/OperationsExample` directory of the repository.