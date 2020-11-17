# Relationships

In order for navigation properties to be identified in the model,
they should be labeled with the appropriate attribute (either `HasOne`, `HasMany` or `HasManyThrough`).

## HasOne

Dependent relationships should contain a property in the form `{RelationshipName}Id`.
For example, a TodoItem may have an Owner and so the Id attribute should be OwnerId.

```c#
public class TodoItem : Identifiable<int>
{
    [Attr]
    public string Description { get; set; }

    [HasOne]
    public Person Owner { get; set; }
    public int OwnerId { get; set; }
}
```

## HasMany

```c#
public class Person : Identifiable<int>
{
    [Attr(PublicName = "first-name")]
    public string FirstName { get; set; }

    [HasMany(PublicName = "todo-items")]
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
    [HasManyThrough(nameof(ArticleTags))] // tells JsonApiDotNetCore to use this as an alias to ArticleTags.Tags
    public ICollection<Tag> Tags { get; set; }

    // this is the Entity Framework Core join relationship
    public ICollection<ArticleTag> ArticleTags { get; set; }
}
```

# Eager loading

_since v4.0_

Your resource may expose a calculated property, whose value depends on a related entity that is not exposed as a json:api resource.
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
