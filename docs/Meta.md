---
currentMenu: meta
---

# Meta

Meta objects can be assigned in two ways:
 - Resource meta
 - Request Meta

Resource meta can be defined by implementing `IHasMeta` on the model class:

```csharp
public class Person : Identifiable<int>, IHasMeta
{
    // ...

    public Dictionary<string, object> GetMeta(IJsonApiContext context)
    {
        return new Dictionary<string, object> {
            { "copyright", "Copyright 2015 Example Corp." },
            { "authors", new string[] { "Jared Nance" } }
        };
    }
}
```

Request Meta can be added by injecting a service that implements `IRequestMeta`.
In the event of a key collision, the Request Meta will take precendence. 
