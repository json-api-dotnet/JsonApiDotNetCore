# Resources

At a minimum, resources must implement `IIdentifiable<TId>` where `TId` is the type of the primary key. The easiest way to do this is to inherit from `Identifiable<TId>`.

```c#
public class Person : Identifiable<Guid>
{
}
```

**Note:** Earlier versions of JsonApiDotNetCore allowed a short-hand notation when `TId` is of type `int`. This was removed in v5.

If you need to attach annotations or attributes on the `Id` property, you can override the virtual property.

```c#
public class Person : Identifiable<int>
{
    [Key]
    [Column("PersonID")]
    public override int Id { get; set; }
}
```

If your resource must inherit from another class,
you can always implement the interface yourself.
In this example, `ApplicationUser` inherits from `IdentityUser`
which already contains an Id property of type string.

```c#
public class ApplicationUser : IdentityUser, IIdentifiable<string>
{
    [NotMapped]
    public string StringId { get => Id; set => Id = value; }
}
```
