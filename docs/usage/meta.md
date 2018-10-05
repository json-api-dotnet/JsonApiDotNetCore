# Metadata

Non-standard metadata can be added to your API responses in 2 ways. Resource and Request meta. In the event of a key collision, the Request Meta will take precendence.

## Resource Meta

Resource Meta is metadata defined on the resource itself by implementing the `IHasMeta` interface.

```c#
public class Person : Identifiable, IHasMeta
{
    public Dictionary<string, object> GetMeta(IJsonApiContext context)
        => new Dictionary<string, object> {
            { "copyright", "Copyright 2018 Example Corp." },
            { "authors", new string[] { "Jared Nance" } }
        };
}
```

## Request Meta

Request Meta can be added by injecting a service that implements `IRequestMeta`. 
This is useful if you need access to other injected services to build the meta object.

```c#
public class RequestMetaService : IRequestMeta
{
    public RequestMetaService(/*...other dependencies here */) {
        // ...
    }

    public Dictionary<string, object> GetMeta(IJsonApiContext context)
        => return new Dictionary<string, object> {
                { "copyright", "Copyright 2018 Example Corp." },
                { "authors", new string[] { "Jared Nance" } }
            };
}
```

```json
{
  "meta": {
    "copyright": "Copyright 2015 Example Corp.",
    "authors": [
      "Jared Nance"
    ]
  },
  "data": {
    // ...
  }
}
```
