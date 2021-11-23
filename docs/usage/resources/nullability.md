# Nullability in resources

Properties on a resource class can be declared as nullable or non-nullable. This affects both ASP.NET ModelState validation and the way Entity Framework Core generates database columns.

ModelState validation is enabled by default since v5.0. In earlier versions, it can be enabled in [options](~/usage/options.md#modelstate-validation).

# Value types

When ModelState validation is enabled, non-nullable value types will **not** trigger a validation error when omitted in the request body.
To make JsonApiDotNetCore return an error when such a property is missing on resource creation, declare it as nullable and annotate it with `[Required]`.

Example:

```c#
public sealed class User : Identifiable<int>
{
    [Attr]
    [Required]
    public bool? IsAdministrator { get; set; }
}
```

This makes Entity Framework Core generate non-nullable columns. And model errors are returned when nullable fields are omitted.

# Reference types

When the [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references) (NRT) compiler feature is enabled, it affects both ASP.NET ModelState validation and Entity Framework Core.

## NRT turned off

When NRT is turned off, use `[Required]` on required attributes and relationships. This makes Entity Framework Core generate non-nullable columns. And model errors are returned when required fields are omitted.

Example:

```c#
#nullable disable

public sealed class Label : Identifiable<int>
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

The [Entity Framework Core guide on NRT](https://docs.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types) recommends to use constructor binding to initialize non-nullable properties, but JsonApiDotNetCore does not support that. For required navigation properties, it suggests to use a non-nullable property with a nullable backing field. JsonApiDotNetCore does not support that either. In both cases, just use the null-forgiving operator (!).

When ModelState validation is turned on, to-many relationships must be assigned an empty collection. Otherwise an error is returned when they don't occur in the request body.

Example:

```c#
#nullable enable

public sealed class Label : Identifiable<int>
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
