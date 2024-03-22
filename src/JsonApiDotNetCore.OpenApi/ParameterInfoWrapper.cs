using System.Reflection;

namespace JsonApiDotNetCore.OpenApi;

/// <summary>
/// Used for parameters in action method expansion. Changes the parameter name and type, while still using all metadata of the underlying non-expanded
/// parameter.
/// </summary>
internal sealed class ParameterInfoWrapper : ParameterInfo
{
    private readonly ParameterInfo _innerParameter;

    public override ParameterAttributes Attributes => _innerParameter.Attributes;
    public override IEnumerable<CustomAttributeData> CustomAttributes => _innerParameter.CustomAttributes;
    public override object? DefaultValue => _innerParameter.DefaultValue;
    public override bool HasDefaultValue => _innerParameter.HasDefaultValue;
    public override MemberInfo Member => _innerParameter.Member;
    public override int MetadataToken => _innerParameter.MetadataToken;
    public override string? Name { get; }
    public override Type ParameterType { get; }
    public override int Position => _innerParameter.Position;
    public override object? RawDefaultValue => _innerParameter.RawDefaultValue;

    public ParameterInfoWrapper(ParameterInfo innerParameter, Type overriddenParameterType, string? overriddenName)
    {
        ArgumentGuard.NotNull(innerParameter);
        ArgumentGuard.NotNull(overriddenParameterType);

        _innerParameter = innerParameter;
        ParameterType = overriddenParameterType;
        Name = overriddenName;
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return _innerParameter.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _innerParameter.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _innerParameter.IsDefined(attributeType, inherit);
    }

    public override bool Equals(object? obj)
    {
        return _innerParameter.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _innerParameter.GetHashCode();
    }

    public override string ToString()
    {
        return _innerParameter.ToString();
    }

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return _innerParameter.GetCustomAttributesData();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return _innerParameter.GetOptionalCustomModifiers();
    }

    public override Type[] GetRequiredCustomModifiers()
    {
        return _innerParameter.GetRequiredCustomModifiers();
    }
}
