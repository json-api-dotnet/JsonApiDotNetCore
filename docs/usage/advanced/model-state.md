# ASP.NET Model Validation

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/InputValidation/ModelState) shows how to use [ASP.NET Model Validation](https://learn.microsoft.com/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api) attributes.

> [!TIP]
> See [Atomic Operations](~/usage/advanced/operations.md) for how to implement a custom model validator.

The resource types are decorated with Model Validation attributes, such as `[Required]`, `[RegularExpression]`, `[MinLength]`, and `[Range]`.

Only the fields that appear in a request body (partial POST/PATCH) are validated.
When validation fails, the source pointer in the response indicates which attribute(s) are invalid.

Model Validation is enabled by default, but can be [turned off in options](~/usage/options.md#modelstate-validation).
Aside from adding validation attributes to your resource properties, no further code is needed.

## Compound and collection attributes

_since v6.0_

Model validation runs on the members of [compound attributes](~/usage/resources/attributes.md#compound-attributes), at every nesting level, and on every element of collections.
Use the same attributes as on resources, such as `[Required]`, `[MaxLength]` and `[Range]`, or implement `IValidatableObject` on a compound type.
The source pointer of an error identifies the invalid member, with array elements addressed by their zero-based index.

```c#
#nullable enable

public class PhoneNumber
{
    [Attr]
    [Required]
    [RegularExpression(@"^\d+(-\d+)*$")]
    public string Number { get; set; } = null!;
}

public class Address : IValidatableObject
{
    [Attr]
    public string City { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (City == "Atlantis")
        {
            yield return new ValidationResult("The city does not exist.", [nameof(City)]);
        }
    }
}
```

```json
{
  "errors": [
    {
      "status": "422",
      "title": "Input validation failed.",
      "detail": "The Number field is required.",
      "source": {
        "pointer": "/data/attributes/phoneNumbers/1/number"
      }
    }
  ]
}
```

- Only attributes that occur in the request body are validated. But because a compound or collection value is always replaced as a whole,
  all nested members of such a value are validated, including the ones the client omitted. See [nullability](~/usage/resources/nullability.md#compound-and-collection-attributes).
- Validation attributes on a collection property apply to the collection itself. For example, `[MaxLength(5)]` on a `List<string>` limits the number of elements.
  The elements of a compound collection are validated individually.
- Validation errors use the public names of attributes in the pointer (which can be customized using `PublicName`), whereas the message uses the property names.
- When `IValidatableObject` is implemented on a compound type, it is invoked for each instance, including each element of a compound collection.
