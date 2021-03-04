# Metadata

We support two ways to add JSON:API meta to your responses: global and per resource.

## Global Meta

Global metadata can be added to the root of the response document by registering a service that implements `IResponseMeta`.
This is useful if you need access to other registered services to build the meta object.

```c#
// In Startup.ConfigureServices
services.AddSingleton<IResponseMeta, CopyrightResponseMeta>();

public sealed class CopyrightResponseMeta : IResponseMeta
{
    public IReadOnlyDictionary<string, object> GetMeta()
    {
        return new Dictionary<string, object>
        {
            ["copyright"] = "Copyright (C) 2002 Umbrella Corporation.",
            ["authors"] = new[] { "Alice", "Red Queen" }
        };
    }
}
```

```json
{
  "meta": {
    "copyright": "Copyright (C) 2002 Umbrella Corporation.",
    "authors": [
      "Alice",
      "Red Queen"
    ]
  },
  "data": []
}
```

## Resource Meta

Resource-specific metadata can be added by implementing `IResourceDefinition<TResource, TId>.GetMeta` (or overriding it on `JsonApiResourceDefinition`):

```c#
public class PersonDefinition : JsonApiResourceDefinition<Person>
{
    public PersonDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override IReadOnlyDictionary<string, object> GetMeta(Person person)
    {
        if (person.IsEmployee)
        {
            return new Dictionary<string, object>
            {
                ["notice"] = "Check our intranet at http://www.example.com/employees/" +
                    person.StringId + " for personal details."
            };
        }

        return null;
    }
}
```

```json
{
  "data": [
    {
      "type": "people",
      "id": "1",
      "attributes": {
        ...
      },
      "meta": {
        "notice": "Check our intranet at http://www.example.com/employees/1 for personal details."
      }
    }
  ]
}
```
