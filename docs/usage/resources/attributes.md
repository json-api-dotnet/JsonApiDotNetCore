# Attributes

If you want an attribute on your model to be publicly available, add the `AttrAttribute`.

```c#
public class Person : Identifiable
{
    [Attr]
    public string FirstName { get; set; }
}
```

## Name

There are two ways the exposed attribute name is determined:

1. Using the configured [naming convention](~/usage/options.md#custom-serializer-settings).

2. Individually using the attribute's constructor.
```c#
public class Person : Identifiable
{
    [Attr(PublicName = "first-name")]
    public string FirstName { get; set; }
}
```

## Capabilities

_since v4.0_

Default JSON:API attribute capabilities are specified in @JsonApiDotNetCore.Configuration.JsonApiOptions#JsonApiDotNetCore_Configuration_JsonApiOptions_DefaultAttrCapabilities:

```c#
options.DefaultAttrCapabilities = AttrCapabilities.None; // default: All
```

This can be overridden per attribute.

### Viewability

Attributes can be marked to allow returning their value in responses. When not allowed and requested using `?fields[]=`, it results in an HTTP 400 response.

```c#
public class User : Identifiable
{
    [Attr(Capabilities = ~AttrCapabilities.AllowView)]
    public string Password { get; set; }
}
```

### Creatability

Attributes can be marked as creatable, which will allow `POST` requests to assign a value to them. When sent but not allowed, an HTTP 422 response is returned.

```c#
public class Person : Identifiable
{
    [Attr(Capabilities = AttrCapabilities.AllowCreate)]
    public string CreatorName { get; set; }
}
```

### Changeability

Attributes can be marked as changeable, which will allow `PATCH` requests to update them. When sent but not allowed, an HTTP 422 response is returned.

```c#
public class Person : Identifiable
{
    [Attr(Capabilities = AttrCapabilities.AllowChange)]
    public string FirstName { get; set; }
}
```

### Filter/Sort-ability

Attributes can be marked to allow filtering and/or sorting. When not allowed, it results in an HTTP 400 response.

```c#
public class Person : Identifiable
{
    [Attr(Capabilities = AttrCapabilities.AllowSort | AttrCapabilities.AllowFilter)]
    public string FirstName { get; set; }
}
```

## Complex Attributes

Models may contain complex attributes.
Serialization of these types is done by [Newtonsoft.Json](https://www.newtonsoft.com/json),
so you should use their APIs to specify serialization formats.
You can also use global options to specify `JsonSerializer` configuration.

```c#
public class Foo : Identifiable
{
    [Attr]
    public Bar Bar { get; set; }
}

public class Bar
{
    [JsonProperty("compound-member")]
    public string CompoundMember { get; set; }
}
```

If you need your complex attributes persisted as a
JSON string in your database, but you need access to it as a concrete type, you can define two members on your resource.
The first member is the concrete type that you will directly interact with in your application. You can use the `NotMapped` attribute to prevent Entity Framework Core from mapping it to the database. The second is the raw JSON property that will be persisted to the database. How you use these members should determine which one is responsible for serialization. In this example, we only serialize and deserialize at the time of persistence
and retrieval.

```c#
public class Foo : Identifiable
{
    [Attr, NotMapped]
    public Bar Bar { get; set; }

    public string BarJson
    {
        get
        {
            return Bar == null ? "{}" : JsonConvert.SerializeObject(Bar);
        }
        set
        {
            Bar = string.IsNullOrWhiteSpace(value)
                ? null
                : JsonConvert.DeserializeObject<Bar>(value);
        }
    }
}
```
