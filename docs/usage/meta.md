# Metadata

Top-level custom metadata can be added to your API responses in two ways: globally and per resource type. In the event of a key collision, the resource meta will take precendence.

## Global Meta

Global metadata can be added by injecting a service that implements `IResponseMeta`.
This is useful if you need access to other injected services to build the meta object.

```c#
public class ResponseMetaService : IResponseMeta
{
    public ResponseMetaService(/*...other dependencies here */) {
        // ...
    }

    public Dictionary<string, object> GetMeta()
    {
        return new Dictionary<string, object>
        {
            {"copyright", "Copyright 2018 Example Corp."},
            {"authors", new string[] {"John Doe"}}
        };
    }
}
```

```json
{
  "meta": {
    "copyright": "Copyright 2018 Example Corp.",
    "authors": [
      "John Doe"
    ]
  },
  "data": {
    // ...
  }
}
```

## Resource Meta

Resource-specific metadata can be added by implementing `IResourceDefinition<TResource, TId>.GetMeta` (or overriding it on JsonApiResourceDefinition):

```c#
public class PersonDefinition : JsonApiResourceDefinition<Person>
{
    public PersonDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
    {
    }

    public override IReadOnlyDictionary<string, object> GetMeta()
    {
        return new Dictionary<string, object>
        {
            ["notice"] = "Check our intranet at http://www.example.com for personal details."
        };
    }
}
```

```json
{
  "meta": {
    "notice": "Check our intranet at http://www.example.com for personal details."
  },
  "data": {
    // ...
  }
}
```
