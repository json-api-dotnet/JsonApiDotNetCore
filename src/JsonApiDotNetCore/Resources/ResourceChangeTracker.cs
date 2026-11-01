using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="IResourceChangeTracker{TResource}" />
[PublicAPI]
public sealed class ResourceChangeTracker<TResource> : IResourceChangeTracker<TResource>
    where TResource : class, IIdentifiable
{
    // Compound and collection attributes are captured as JSON, so that changes to members that are not exposed as attributes (for example a last-update
    // timestamp inside a compound type) are detected too. This uses private options, independent of the application's serializer options: public
    // properties and fields only, no naming policy, no converters, nothing skipped (nulls and default values are written), recursion is cut off, and NaN and
    // Infinity are representable. Limitations: members hidden through [JsonIgnore] (and non-public members) cannot be compared this way. Sets are written
    // in enumeration order, so a set that enumerates in a different order than before can show up as a change.
    private static readonly JsonSerializerOptions SnapshotOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        IncludeFields = true,
        WriteIndented = false
    };

    private readonly IJsonApiRequest _request;
    private readonly ITargetedFields _targetedFields;

    private Dictionary<string, object?>? _initiallyStoredAttributeValues;
    private Dictionary<string, object?>? _requestAttributeValues;
    private Dictionary<string, object?>? _finallyStoredAttributeValues;

    public ResourceChangeTracker(IJsonApiRequest request, ITargetedFields targetedFields)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(targetedFields);

        _request = request;
        _targetedFields = targetedFields;
    }

    /// <inheritdoc />
    public void SetInitiallyStoredAttributeValues(TResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _initiallyStoredAttributeValues = CreateAttributeDictionary(resource, _request.PrimaryResourceType!.Attributes);
    }

    /// <inheritdoc />
    public void SetRequestAttributeValues(TResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _requestAttributeValues = CreateAttributeDictionary(resource, _targetedFields.Attributes);
    }

    /// <inheritdoc />
    public void SetFinallyStoredAttributeValues(TResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _finallyStoredAttributeValues = CreateAttributeDictionary(resource, _request.PrimaryResourceType!.Attributes);
    }

    private Dictionary<string, object?> CreateAttributeDictionary(TResource resource, IEnumerable<AttrAttribute> attributes)
    {
        var result = new Dictionary<string, object?>();

        foreach (AttrAttribute attribute in attributes)
        {
            result.Add(attribute.PublicName, CreateSnapshot(attribute, resource));
        }

        return result;
    }

    private static object? CreateSnapshot(AttrAttribute attribute, TResource resource)
    {
        object? value = attribute.GetValue(resource);

        if (value == null || (!attribute.IsCompound && !attribute.IsCollection))
        {
            // Other values keep their own equality semantics.
            return value;
        }

        // Captured now, so that later in-place changes to the instance do not affect the snapshot.
        return JsonSerializer.SerializeToElement(value, value.GetType(), SnapshotOptions);
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left is JsonElement leftElement && right is JsonElement rightElement)
        {
            return JsonElement.DeepEquals(leftElement, rightElement);
        }

        return Equals(left, right);
    }

    /// <inheritdoc />
    public bool HasImplicitChanges()
    {
        if (_initiallyStoredAttributeValues != null && _requestAttributeValues != null && _finallyStoredAttributeValues != null)
        {
            foreach (string key in _initiallyStoredAttributeValues.Keys)
            {
                if (_requestAttributeValues.TryGetValue(key, out object? requestValue))
                {
                    object? actualValue = _finallyStoredAttributeValues[key];

                    if (!AreEqual(requestValue, actualValue))
                    {
                        return true;
                    }
                }
                else
                {
                    object? initiallyStoredValue = _initiallyStoredAttributeValues[key];
                    object? finallyStoredValue = _finallyStoredAttributeValues[key];

                    if (!AreEqual(initiallyStoredValue, finallyStoredValue))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
