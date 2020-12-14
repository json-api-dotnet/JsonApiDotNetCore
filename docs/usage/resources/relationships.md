# Relationships

In order for navigation properties to be identified in the model,
they should be labeled with the appropriate attribute (either `HasOne`, `HasMany` or `HasManyThrough`).

## HasOne

This exposes a to-one relationship.

```c#
public class TodoItem : Identifiable
{
    [HasOne]
    public Person Owner { get; set; }
}
```

## HasMany

This exposes a to-many relationship.

```c#
public class Person : Identifiable
{
    [HasMany]
    public ICollection<TodoItem> TodoItems { get; set; }
}
```

## HasManyThrough

Currently, Entity Framework Core [does not support](https://github.com/aspnet/EntityFrameworkCore/issues/1368) many-to-many relationships without a join entity.
For this reason, we have decided to fill this gap by allowing applications to declare a relationship as `HasManyThrough`.
JsonApiDotNetCore will expose this relationship to the client the same way as any other `HasMany` attribute.
However, under the covers it will use the join type and Entity Framework Core's APIs to get and set the relationship.

```c#
public class Article : Identifiable
{
    [NotMapped] // tells Entity Framework Core to ignore this property
    [HasManyThrough(nameof(ArticleTags))] // tells JsonApiDotNetCore to use the join table below
    public ICollection<Tag> Tags { get; set; }

    // this is the Entity Framework Core navigation to the join table
    public ICollection<ArticleTag> ArticleTags { get; set; }
}
```

## Name

There are two ways the exposed relationship name is determined:

1. Using the configured [naming convention](~/usage/options.md#custom-serializer-settings).

2. Individually using the attribute's constructor.
```c#
public class TodoItem : Identifiable
{
    [HasOne(PublicName = "item-owner")]
    public Person Owner { get; set; }
}
```

## Includibility

Relationships can be marked to disallow including them using the `?include=` query string parameter. When not allowed, it results in an HTTP 400 response.

```c#
public class TodoItem : Identifiable
{
    [HasOne(CanInclude: false)]
    public Person Owner { get; set; }
}
```

# Eager loading

_since v4.0_

Your resource may expose a calculated property, whose value depends on a related entity that is not exposed as a JSON:API resource.
So for the calculated property to be evaluated correctly, the related entity must always be retrieved. You can achieve that using `EagerLoad`, for example:

```c#
public class ShippingAddress : Identifiable
{
    [Attr]
    public string Street { get; set; }

    [Attr]
    public string CountryName
    {
        get { return Country.DisplayName; }
    }

    [EagerLoad] // not exposed as resource, but adds .Include("Country") to the query
    public Country Country { get; set; }
}

public class Country
{
    public string IsoCode { get; set; }
    public string DisplayName { get; set; }
}
```
