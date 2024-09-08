using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc cref="IResourceChangeTracker{TResource}" />
[PublicAPI]
public sealed class ResourceChangeTracker<TResource> : IResourceChangeTracker<TResource>
    where TResource : class, IIdentifiable
{
    private readonly IJsonApiRequest _request;
    private readonly ITargetedFields _targetedFields;

    private Dictionary<string, object?>? _initiallyStoredAttributeValues;
    private Dictionary<string, object?>? _requestAttributeValues;
    private Dictionary<string, object?>? _finallyStoredAttributeValues;

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

    private Dictionary<string, object?> CreateAttributeDictionary(TResource resource, IEnumerable<AttrAttribute> attributes)
    {
        var result = new Dictionary<string, object?>();

        foreach (AttrAttribute attribute in attributes)
        {
            object? value = attribute.GetValue(resource);
            result.Add(attribute.PublicName, value);
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
                if (_requestAttributeValues.TryGetValue(key, out object? requestValue))
                {
                    object? actualValue = _finallyStoredAttributeValues[key];

                    if (!Equals(requestValue, actualValue))
                    {
                        return true;
                    }
                }
                else
                {
                    object? initiallyStoredValue = _initiallyStoredAttributeValues[key];
                    object? finallyStoredValue = _finallyStoredAttributeValues[key];

                    if (!Equals(initiallyStoredValue, finallyStoredValue))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
