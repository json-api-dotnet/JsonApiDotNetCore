# Attributes

If you want an attribute on your model to be publicly available, add the `AttrAttribute`.

```c#
#nullable enable

public class Person : Identifiable<int>
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public string LastName { get; set; } = null!;
}
```

## Name

There are two ways the exposed attribute name is determined:

1. Using the configured [naming convention](~/usage/options.md#customize-serializer-options).

2. Individually using the attribute's constructor.
```c#
#nullable enable
public class Person : Identifiable<int>
{
    [Attr(PublicName = "first-name")]
    public string? FirstName { get; set; }
}
```

## Capabilities

_since v4.0_

Default JSON:API attribute capabilities are specified in @JsonApiDotNetCore.Configuration.JsonApiOptions#JsonApiDotNetCore_Configuration_JsonApiOptions_DefaultAttrCapabilities:

```c#
options.DefaultAttrCapabilities = AttrCapabilities.None; // default: All
```

This can be overridden per attribute.

### AllowView

Indicates whether the attribute value can be returned in responses. When not allowed and requested using `?fields[]=`, it results in an HTTP 400 response.
Otherwise, the attribute is silently omitted.

```c#
#nullable enable

public class User : Identifiable<int>
{
    [Attr(Capabilities = ~AttrCapabilities.AllowView)]
    public string Password { get; set; } = null!;
}
```

### AllowFilter

Indicates whether the attribute can be filtered on. When not allowed and used in `?filter=`, an HTTP 400 is returned.

```c#
#nullable enable

public class Person : Identifiable<int>
{
    [Attr(Capabilities = AttrCapabilities.AllowFilter)]
    public string? FirstName { get; set; }
}
```

### AllowSort

Indicates whether the attribute can be sorted on. When not allowed and used in `?sort=`, an HTTP 400 is returned.

```c#
#nullable enable

public class Person : Identifiable<int>
{
    [Attr(Capabilities = ~AttrCapabilities.AllowSort)]
    public string? FirstName { get; set; }
}
```

### AllowCreate

Indicates whether POST requests can assign the attribute value. When sent but not allowed, an HTTP 422 response is returned.

```c#
#nullable enable

public class Person : Identifiable<int>
{
    [Attr(Capabilities = AttrCapabilities.AllowCreate)]
    public string? CreatorName { get; set; }
}
```

### AllowChange

Indicates whether PATCH requests can update the attribute value. When sent but not allowed, an HTTP 422 response is returned.

```c#
#nullable enable

public class Person : Identifiable<int>
{
    [Attr(Capabilities = AttrCapabilities.AllowChange)]
    public string? FirstName { get; set; };
}
```

## Complex Attributes

Models may contain complex attributes.
Serialization of these types is done by [System.Text.Json](https://www.nuget.org/packages/System.Text.Json),
so you should use their APIs to specify serialization format.
You can also use [global options](~/usage/options.md#customize-serializer-options) to control the `JsonSerializer` behavior.

```c#
#nullable enable

public class Foo : Identifiable<int>
{
    [Attr]
    public Bar? Bar { get; set; }
}

public class Bar
{
    [JsonPropertyName("compound-member")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CompoundMember { get; set; }
}
```

If you need your complex attributes persisted as a
JSON string in your database, but you need access to it as a concrete type, you can define two members on your resource.
The first member is the concrete type that you will directly interact with in your application. You can use the `NotMapped` attribute to prevent Entity Framework Core from mapping it to the database. The second is the raw JSON property that will be persisted to the database. How you use these members should determine which one is responsible for serialization. In this example, we only serialize and deserialize at the time of persistence
and retrieval.

```c#
#nullable enable

public class Foo : Identifiable<int>
{
    [Attr]
    [NotMapped]
    public Bar? Bar { get; set; }

    public string? BarJson
    {
        get
        {
            return Bar == null ? "{}" : JsonSerializer.Serialize(Bar);
        }
        set
        {
            Bar = string.IsNullOrWhiteSpace(value)
                ? null
                : JsonSerializer.Deserialize<Bar>(value);
        }
    }
}
```
