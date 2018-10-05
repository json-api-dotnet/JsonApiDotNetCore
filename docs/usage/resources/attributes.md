# Attributes

If you want an attribute on your model to be publicly available, add the `AttrAttribute`.

```c#
public class Person : Identifiable
{
    [Attr]
    public string FirstName { get; set; }
}
```

## Public name

There are two ways the public attribute name is determined:
1. By convention, specified by @JsonApiDotNetCore.Configuration.JsonApiOptions#JsonApiDotNetCore_Configuration_JsonApiOptions_ResourceNameFormatter
```c#
options.ResourceNameFormatter = new DefaultResourceNameFormatter();
```

2. Individually using the attribute's constructor
```c#
public class Person : Identifiable
{
    [Attr("first-name")]
    public string FirstName { get; set; }
}
```

## Immutability

Attributes can be marked as immutable which will prevent `PATCH` requests from updating them.

```c#
public class Person : Identifiable<int>
{
    [Attr(immutable: true)]
    public string FirstName { get; set; }
}
```

## Filter|Sort-ability

All attributes are filterable and sortable by default. 
You can disable this by setting `IsFiterable` and `IsSortable` to `false `.
Requests to filter or sort these attributes will receive an HTTP 400 response.

```c#
public class Person : Identifiable<int>
{
    [Attr(isFilterable: false, isSortable: false)]
    public string FirstName { get; set; }
}
```

## Complex Attributes

Models may contain complex attributes.
Serialization of these types is done by Newtonsoft.Json,
so you should use their APIs to specify serialization formats.
You can also use global options to specify the `JsonSerializer` that gets used.

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
The first member is the concrete type that you will directly interact with in your application. We can use the `NotMapped` attribute to prevent Entity Framework from mapping it to the database. The second is the raw JSON property that will be persisted to the database. How you use these members should determine which one is responsible for serialization. In this example, we only serialize and deserialize at the time of persistence
and retrieval.

```c#
public class Foo : Identifiable
{
    [Attr, NotMapped]
    public Bar Bar { get; set; }

    public string BarJson 
    { 
        get => (Bar == null) 
                ? "{}"
                : JsonConvert.SerializeObject(Bar);
        
        set => Bar = string.IsNullOrWhiteSpace(value)
                ? null
                : JsonConvert.DeserializeObject(value);
    };
}
```