# Relationships

A relationship is a named link between two resource types, including a direction.
They are similar to [navigation properties in Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/modeling/relationships).

Relationships come in three flavors: to-one, to-many and many-to-many.
The left side of a relationship is where the relationship is declared, the right side is the resource type it points to.

## HasOne

This exposes a to-one relationship.

```c#
public class TodoItem : Identifiable
{
    [HasOne]
    public Person Owner { get; set; }
}
```

The left side of this relationship is of type `TodoItem` (public name: "todoItems") and the right side is of type `Person` (public name: "persons").

## HasMany

This exposes a to-many relationship.

```c#
public class Person : Identifiable
{
    [HasMany]
    public ICollection<TodoItem> TodoItems { get; set; }
}
```

The left side of this relationship is of type `Person` (public name: "persons") and the right side is of type `TodoItem` (public name: "todoItems").

## HasManyThrough

_removed since v5.0_

Earlier versions of Entity Framework Core (up to v5) [did not support](https://github.com/aspnet/EntityFrameworkCore/issues/1368) many-to-many relationships without a join entity.
For this reason, earlier versions of JsonApiDotNetCore filled this gap by allowing applications to declare a relationship as `HasManyThrough`,
which would expose the relationship to the client the same way as any other `HasMany` relationship.
However, under the covers it would use the join type and Entity Framework Core's APIs to get and set the relationship.

```c#
public class Article : Identifiable
{
    // tells Entity Framework Core to ignore this property
    [NotMapped]

    // tells JsonApiDotNetCore to use the join table below
    [HasManyThrough(nameof(ArticleTags))]
    public ICollection<Tag> Tags { get; set; }

    // this is the Entity Framework Core navigation to the join table
    public ICollection<ArticleTag> ArticleTags { get; set; }
}
```

The left side of this relationship is of type `Article` (public name: "articles") and the right side is of type `Tag` (public name: "tags").

## Name

There are two ways the exposed relationship name is determined:

1. Using the configured [naming convention](~/usage/options.md#customize-serializer-options).

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

    // not exposed as resource, but adds .Include("Country") to the query
    [EagerLoad]
    public Country Country { get; set; }
}

public class Country
{
    public string IsoCode { get; set; }
    public string DisplayName { get; set; }
}
```
