# Nullability in resources

Properties on a resource class can be declared as nullable or non-nullable. This affects both ASP.NET ModelState validation and the way Entity Framework Core generates database columns.

ModelState validation is enabled by default since v5.0. In earlier versions, it can be enabled in [options](~/usage/options.md#modelstate-validation).

# Value types

When ModelState validation is enabled, non-nullable value types will **not** trigger a validation error when omitted in the request body.
To make JsonApiDotNetCore return an error when such a property is missing on resource creation, declare it as nullable and annotate it with `[Required]`.

Example:

```c#
public sealed class User : Identifiable<long>
{
    [Attr]
    [Required]
    public bool? IsAdministrator { get; set; }
}
```

This makes Entity Framework Core generate non-nullable columns. And model errors are returned when nullable fields are omitted.

# Reference types

When the [nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references) (NRT) compiler feature is enabled, it affects both ASP.NET ModelState validation and Entity Framework Core.

## NRT turned off

When NRT is turned off, use `[Required]` on required attributes and relationships. This makes Entity Framework Core generate non-nullable columns. And model errors are returned when required fields are omitted.

Example:

```c#
#nullable disable

public sealed class Label : Identifiable<long>
{
    [Attr]
    [Required]
    public string Name { get; set; }

    [Attr]
    public string RgbColor { get; set; }

    [HasOne]
    [Required]
    public Person Creator { get; set; }

    [HasOne]
    public Label Parent { get; set; }

    [HasMany]
    public ISet<TodoItem> TodoItems { get; set; }
}
```

## NRT turned on

When NRT is turned on, use nullability annotations (?) on attributes and relationships. This makes Entity Framework Core generate non-nullable columns. And model errors are returned when non-nullable fields are omitted.

The [Entity Framework Core guide on NRT](https://learn.microsoft.com/ef/core/miscellaneous/nullable-reference-types) recommends using constructor binding to initialize non-nullable properties, but JsonApiDotNetCore does not support that. For required navigation properties, it suggests using a non-nullable property with a nullable backing field. JsonApiDotNetCore does not support that either. In both cases, use the null-forgiving operator (!).

When ModelState validation is turned on, to-many relationships must be assigned an empty collection. Otherwise an error is returned when they don't occur in the request body.

Example:

```c#
#nullable enable

public sealed class Label : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? RgbColor { get; set; }

    [HasOne]
    public Person Creator { get; set; } = null!;

    [HasOne]
    public Label? Parent { get; set; }

    [HasMany]
    public ISet<TodoItem> TodoItems { get; set; } = new HashSet<TodoItem>();
}
```

# Compound and collection attributes

_since v6.0_

The rules above apply to the members of [compound attributes](~/usage/resources/attributes.md#compound-attributes) as well, at every nesting level.
Errors on nested members have a source pointer that identifies the member, for example `/data/attributes/name/lastName`.

```c#
#nullable enable

public sealed class Contact : Identifiable<long>
{
    // Required: sending null results in an error.
    [Attr(IsCompound = true)]
    public ContactName Name { get; set; } = null!;

    // Optional: can be set to null.
    [Attr(IsCompound = true)]
    public Address? HomeAddress { get; set; }

    // Required collection, null is not accepted. Use List<PhoneNumber>? to allow null.
    [Attr(IsCompound = true)]
    public List<PhoneNumber> PhoneNumbers { get; set; } = [];

    // Optional scalar collection. The elements can be null too.
    [Attr]
    public List<int?>? Ratings { get; set; }
}

public class ContactName
{
    [Attr]
    public string? FirstName { get; set; }

    // Required within the compound value.
    [Attr]
    public string LastName { get; set; } = null!;
}
```

- A non-nullable compound attribute (or compound member) that is set to `null` in a request body results in an HTTP 422 response,
  such as `The Name field is required.` with a pointer to the attribute. A nullable one accepts `null`.
- Because a PATCH replaces the whole compound value, the nullability rules of all its members are validated when the compound occurs in the request body,
  even for members the client did not intend to change. Compound attributes that do not occur in the request body are not validated.
- For a struct, use a nullable struct (`GeoPoint?`) to allow `null`. A non-nullable struct is a value type, so omitting it or its members does not trigger an error.
  Use `[Required]` on a nullable struct (or nullable member) to require it.
- A non-nullable collection that is set to `null` results in an error, like any other non-nullable reference type.
  A nullable collection (`List<string>?`) accepts `null`, which is different from an empty collection.
  In filters and when sorting by count, a `null` collection counts as empty.
- Elements of a collection can be `null` when the element type is a nullable value type (`int?`) or a reference type.
  For non-nullable value types (such as `int`), a `null` element results in an HTTP 422 response.
