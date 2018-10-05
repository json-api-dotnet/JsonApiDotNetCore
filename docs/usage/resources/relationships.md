# Relationships

In order for navigation properties to be identified in the model, 
they should be labeled with the appropriate attribute (either `HasOne`, `HasMany` or `HasManyThrough`).

## HasOne

Dependent relationships should contain a property in the form `{RelationshipName}Id`. 
For example, a TodoItem may have an Owner and so the Id attribute should be OwnerId.

```c#
public class TodoItem : Identifiable<int>
{
    [Attr("description")]
    public string Description { get; set; }

    [HasOne("owner")]
    public virtual Person Owner { get; set; }
    public int OwnerId { get; set; }
}
```

The convention used used to locate the foreign key property (e.g. `OwnerId`) can be changed on
the @JsonApiDotNetCore.Configuration.JsonApiOptions#JsonApiDotNetCore_Configuration_JsonApiOptions_RelatedIdMapper

## HasMany

```c#
public class Person : Identifiable<int>
{
    [Attr("first-name")]
    public string FirstName { get; set; }

    [HasMany("todo-items")]
    public virtual List<TodoItem> TodoItems { get; set; }
}
```

## HasManyThrough

Currently EntityFrameworkCore [does not support](https://github.com/aspnet/EntityFrameworkCore/issues/1368) Many-to-Many relationships without a join entity. 
For this reason, we have decided to fill this gap by allowing applications to declare a relationships as `HasManyThrough`. 
JsonApiDotNetCore will expose this attribute to the client the same way as any other `HasMany` attribute.
However, under the covers it will use the join type and EntityFramework's APIs to get and set the relationship.

```c#
public class Article : Identifiable
{
    [NotMapped] // ← tells EF to ignore this property
    [HasManyThrough(nameof(ArticleTags))] // ← tells JADNC to use this as an alias to ArticleTags.Tags
    public List<Tag> Tags { get; set; }

    // this is the EF join relationship
    public List<ArticleTag> ArticleTags { get; set; }
}
```