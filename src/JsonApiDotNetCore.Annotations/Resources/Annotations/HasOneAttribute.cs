using JetBrains.Annotations;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Used to expose a property on a resource class as a JSON:API to-one relationship (https://jsonapi.org/format/#document-resource-object-relationships).
/// </summary>
/// <example>
/// <code><![CDATA[
/// public class Article : Identifiable
/// {
///     [HasOne]
///     public Author Author { get; set; }
/// }
/// ]]></code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public sealed class HasOneAttribute : RelationshipAttribute
{
    private readonly Lazy<bool> _lazyIsOneToOne;
    private HasOneCapabilities? _capabilities;

    /// <summary>
    /// Inspects <see cref="RelationshipAttribute.InverseNavigationProperty" /> to determine if this is a one-to-one relationship.
    /// </summary>
    internal bool IsOneToOne => _lazyIsOneToOne.Value;

    internal bool HasExplicitCapabilities => _capabilities != null;

    /// <summary>
    /// The set of allowed capabilities on this to-one relationship. When not explicitly set, the configured default set of capabilities is used.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public class Book : Identifiable<long>
    /// {
    ///     [HasOne(Capabilities = HasOneCapabilities.AllowView | HasOneCapabilities.AllowInclude)]
    ///     public Person? Author { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    public HasOneCapabilities Capabilities
    {
        get => _capabilities ?? default;
        set => _capabilities = value;
    }

    public HasOneAttribute()
    {
        _lazyIsOneToOne = new Lazy<bool>(EvaluateIsOneToOne, LazyThreadSafetyMode.PublicationOnly);
    }

    private bool EvaluateIsOneToOne()
    {
        if (InverseNavigationProperty != null)
        {
            Type? elementType = CollectionConverter.FindCollectionElementType(InverseNavigationProperty.PropertyType);
            return elementType == null;
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (HasOneAttribute)obj;

        return _capabilities == other._capabilities && base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_capabilities, base.GetHashCode());
    }
}
