namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Indicates the kind of <see cref="AttrAttribute" />.
/// </summary>
public enum AttrKind
{
    /// <summary>
    /// A primitive attribute, such as <see cref="int" />, <see cref="string" />, <see cref="Enum" />, <see cref="Guid" />, <see cref="DateTime" /> or
    /// <see cref="TimeSpan" />.
    /// </summary>
    Primitive,

    /// <summary>
    /// An attribute that contains nested attributes, such as Contact, Address or PhoneNumber.
    /// </summary>
    Compound,

    /// <summary>
    /// A collection of primitive attributes.
    /// </summary>
    CollectionOfPrimitive,

    /// <summary>
    /// A collection of compound attributes.
    /// </summary>
    CollectionOfCompound
}
