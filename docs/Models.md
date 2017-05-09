---
currentMenu: models
---

# Defining Models

Models must implement [IIdentifiable&lt;TId&gt;](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Models/IIdentifiable.cs).
The easiest way to do this is to inherit [Identifiable&lt;TId&gt;](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Models/Identifiable.cs) where `TId` is the type of the primary key, like so:

```csharp
public class Person : Identifiable<Guid>
{ }
```

You can use the non-generic `Identifiable` if your primary key is an integer:

```csharp
public class Person : Identifiable
{ }
```

If you need to hang annotations or attributes on the `Id` property, you can override the virtual member:

```csharp
public class Person : Identifiable
{ 
    [Key]
    [Column("person_id")]
    public override int Id { get; set; }
}
```

If your model must inherit from another class, you can always implement the interface yourself.
In the following example, ApplicationUser inherits IdentityUser which already contains an Id property of
type string.

```csharp
public class ApplicationUser 
: IdentityUser, IIdentifiable<string>
{
    [NotMapped]
    public string StringId { get => this.Id; set => Id = value; }
}
```

## Specifying Public Attributes

If you want an attribute on your model to be publicly available, 
add the `AttrAttribute` and provide the outbound name.

```csharp
public class Person : Identifiable<int>
{
    [Attr("first-name")]
    public string FirstName { get; set; }
}
```

## Relationships

In order for navigation properties to be identified in the model, 
they should be labeled with the appropriate attribute (either `HasOne` or `HasMany`).

```csharp
public class Person : Identifiable<int>
{
    [Attr("first-name")]
    public string FirstName { get; set; }

    [HasMany("todo-items")]
    public virtual List<TodoItem> TodoItems { get; set; }
}
```

Dependent relationships should contain a property in the form `{RelationshipName}Id`. 
For example, a `TodoItem` may have an `Owner` and so the Id attribute should be `OwnerId` like so:

```csharp
public class TodoItem : Identifiable<int>
{
    [Attr("description")]
    public string Description { get; set; }

    public int OwnerId { get; set; }

    [HasOne("owner")]
    public virtual Person Owner { get; set; }
}
```

## Resource Names

See [ContextGraph](contextGraph.html) for details on how the resource names are determined.