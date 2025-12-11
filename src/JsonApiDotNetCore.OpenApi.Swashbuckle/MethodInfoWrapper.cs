using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Used to hide custom attributes that are applied on a method from Swashbuckle.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MethodInfoWrapper : MethodInfo
{
    private readonly MethodInfo _innerMethod;
    private readonly Type[] _attributeTypesToHide;

    public override IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();
    public override Type? DeclaringType => _innerMethod.DeclaringType;
    public override bool IsCollectible => _innerMethod.IsCollectible;
    public override int MetadataToken => _innerMethod.MetadataToken;
    public override Module Module => _innerMethod.Module;
    public override string Name => _innerMethod.Name;
    public override Type? ReflectedType => _innerMethod.ReflectedType;
    public override MethodAttributes Attributes => _innerMethod.Attributes;
    public override CallingConventions CallingConvention => _innerMethod.CallingConvention;
    public override bool ContainsGenericParameters => _innerMethod.ContainsGenericParameters;
    public override bool IsConstructedGenericMethod => _innerMethod.IsConstructedGenericMethod;
    public override bool IsGenericMethod => _innerMethod.IsGenericMethod;
    public override bool IsGenericMethodDefinition => _innerMethod.IsGenericMethodDefinition;
    public override bool IsSecurityCritical => _innerMethod.IsSecurityCritical;
    public override bool IsSecuritySafeCritical => _innerMethod.IsSecuritySafeCritical;
    public override bool IsSecurityTransparent => _innerMethod.IsSecurityTransparent;
    public override RuntimeMethodHandle MethodHandle => _innerMethod.MethodHandle;
    public override MethodImplAttributes MethodImplementationFlags => _innerMethod.MethodImplementationFlags;
    public override MemberTypes MemberType => _innerMethod.MemberType;
    public override ParameterInfo ReturnParameter => _innerMethod.ReturnParameter;
    public override Type ReturnType => _innerMethod.ReturnType;
    public override ICustomAttributeProvider ReturnTypeCustomAttributes => _innerMethod.ReturnTypeCustomAttributes;

    public MethodInfoWrapper(MethodInfo innerMethod, Type[] attributeTypesToHide)
    {
        ArgumentNullException.ThrowIfNull(innerMethod);
        ArgumentGuard.NotNullNorEmpty(attributeTypesToHide);

        _innerMethod = innerMethod;
        _attributeTypesToHide = attributeTypesToHide;
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        List<object> customAttributes = _innerMethod.GetCustomAttributes(inherit).ToList();

#pragma warning disable AV1530 // Loop variable should not be written to in loop body
        for (int index = 0; index < customAttributes.Count; index++)
        {
            if (_attributeTypesToHide.Any(attribute => attribute.IsInstanceOfType(customAttributes[index])))
            {
                customAttributes.RemoveAt(index);
                index--;
            }
        }
#pragma warning restore AV1530 // Loop variable should not be written to in loop body

        return customAttributes.ToArray();
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        List<object> customAttributes = _innerMethod.GetCustomAttributes(attributeType, inherit).ToList();

#pragma warning disable AV1530 // Loop variable should not be written to in loop body
        for (int index = 0; index < customAttributes.Count; index++)
        {
            if (_attributeTypesToHide.Any(attribute => attribute.IsInstanceOfType(customAttributes[index])))
            {
                customAttributes.RemoveAt(index);
                index--;
            }
        }
#pragma warning restore AV1530 // Loop variable should not be written to in loop body

        object[] typedArray = (object[])Array.CreateInstance(attributeType, customAttributes.Count);

        for (int index = 0; index < customAttributes.Count; index++)
        {
            typedArray[index] = customAttributes[index];
        }

        return typedArray;
    }

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        List<CustomAttributeData> customAttributes = _innerMethod.GetCustomAttributesData().ToList();

#pragma warning disable AV1530 // Loop variable should not be written to in loop body
        for (int index = 0; index < customAttributes.Count; index++)
        {
            if (_attributeTypesToHide.Any(attribute => attribute.IsAssignableFrom(customAttributes[index].AttributeType)))
            {
                customAttributes.RemoveAt(index);
                index--;
            }
        }
#pragma warning restore AV1530 // Loop variable should not be written to in loop body

        return customAttributes.ToArray();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return _innerMethod.HasSameMetadataDefinitionAs(other);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _innerMethod.IsDefined(attributeType, inherit);
    }

    public override MethodBody? GetMethodBody()
    {
        return _innerMethod.GetMethodBody();
    }

    public override MethodImplAttributes GetMethodImplementationFlags()
    {
        return _innerMethod.GetMethodImplementationFlags();
    }

    public override ParameterInfo[] GetParameters()
    {
        return _innerMethod.GetParameters();
    }

    public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture)
    {
        return _innerMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
    }

    public override Delegate CreateDelegate(Type delegateType)
    {
        return _innerMethod.CreateDelegate(delegateType);
    }

    public override Delegate CreateDelegate(Type delegateType, object? target)
    {
        return _innerMethod.CreateDelegate(delegateType, target);
    }

    public override MethodInfo GetBaseDefinition()
    {
        return _innerMethod.GetBaseDefinition();
    }

    public override Type[] GetGenericArguments()
    {
        return _innerMethod.GetGenericArguments();
    }

    public override MethodInfo GetGenericMethodDefinition()
    {
        return _innerMethod.GetGenericMethodDefinition();
    }

    public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
    {
        return _innerMethod.MakeGenericMethod(typeArguments);
    }
}
