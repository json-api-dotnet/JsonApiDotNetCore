using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc />
[PublicAPI]
public sealed class ResourceChangeTracker<TResource> : IResourceChangeTracker<TResource>
    where TResource : class, IIdentifiable
{
    private readonly IJsonApiRequest _request;
    private readonly ITargetedFields _targetedFields;

    private IDictionary<string, string?>? _initiallyStoredAttributeValues;
    private IDictionary<string, string?>? _requestAttributeValues;
    private IDictionary<string, string?>? _finallyStoredAttributeValues;

    public ResourceChangeTracker(IJsonApiRequest request, ITargetedFields targetedFields)
    {
        ArgumentGuard.NotNull(request);
        ArgumentGuard.NotNull(targetedFields);

        _request = request;
        _targetedFields = targetedFields;
    }

    /// <inheritdoc />
    public void SetInitiallyStoredAttributeValues(TResource resource)
    {
        ArgumentGuard.NotNull(resource);

        _initiallyStoredAttributeValues = CreateAttributeDictionary(resource, _request.PrimaryResourceType!.Attributes);
    }

    /// <inheritdoc />
    public void SetRequestAttributeValues(TResource resource)
    {
        ArgumentGuard.NotNull(resource);

        _requestAttributeValues = CreateAttributeDictionary(resource, _targetedFields.Attributes);
    }

    /// <inheritdoc />
    public void SetFinallyStoredAttributeValues(TResource resource)
    {
        ArgumentGuard.NotNull(resource);

        _finallyStoredAttributeValues = CreateAttributeDictionary(resource, _request.PrimaryResourceType!.Attributes);
    }

    private IDictionary<string, string?> CreateAttributeDictionary(TResource resource, IEnumerable<AttrAttribute> attributes)
    {
        var result = new Dictionary<string, string?>();

        foreach (AttrAttribute attribute in attributes)
        {
            object? value = attribute.GetValue(resource);
            string json = JsonSerializer.Serialize(value);
            result.Add(attribute.PublicName, json);
        }

        if (resource is IVersionedIdentifiable versionedIdentifiable)
        {
            result.Add(nameof(versionedIdentifiable.Version), versionedIdentifiable.Version);
        }

        return result;
    }

    /// <inheritdoc />
    public bool HasImplicitChanges()
    {
        if (_initiallyStoredAttributeValues != null && _requestAttributeValues != null && _finallyStoredAttributeValues != null)
        {
            foreach (string key in _initiallyStoredAttributeValues.Keys)
            {
                if (_requestAttributeValues.TryGetValue(key, out string? requestValue))
                {
                    string? actualValue = _finallyStoredAttributeValues[key];

                    if (requestValue != actualValue)
                    {
                        return true;
                    }
                }
                else
                {
                    string? initiallyStoredValue = _initiallyStoredAttributeValues[key];
                    string? finallyStoredValue = _finallyStoredAttributeValues[key];

                    if (initiallyStoredValue != finallyStoredValue)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
