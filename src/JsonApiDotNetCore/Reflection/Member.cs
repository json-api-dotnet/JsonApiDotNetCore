using JsonApiDotNetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JsonApiDotNetCore.Reflection
{
    [Serializable]
    public abstract class Member: IEquatable<Member>
    {
        public abstract string Name { get; }
        public abstract Type PropertyType { get; }
        public abstract bool CanWrite { get; }
        public abstract MemberInfo MemberInfo { get; }
        public abstract Type DeclaringType { get; }
        public abstract bool HasIndexParameters { get; }
        public abstract bool IsMethod { get; }
        public abstract bool IsField { get; }
        public abstract bool IsProperty { get; }
        public abstract bool IsAutoProperty { get; }
        public abstract bool IsPrivate { get; }
        public abstract bool IsProtected { get; }
        public abstract bool IsPublic { get; }
        public abstract bool IsInternal { get; }

        public bool Equals(Member other)
        {
            return Equals(other.MemberInfo.MetadataToken, MemberInfo.MetadataToken) && Equals(other.MemberInfo.Module, MemberInfo.Module);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            { 
                return true; 
            }

            if (!(obj is Member))
            {
                return false;
            }

            return Equals((Member)obj);
        }

        public override int GetHashCode()
        {
            return MemberInfo.GetHashCode() ^ 3;
        }

        public static bool operator ==(Member left, Member right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Member left, Member right)
        {
            return !Equals(left, right);
        }

        public abstract void SetValue(object target, object value);
        public abstract object GetValue(object target);
        public abstract bool TryGetBackingField(out Member backingField);
    }

    [Serializable]
    internal class MethodMember: Member
    {
        private readonly MethodInfo _member;
        private Member _backingField;

        public override void SetValue(object target, object value)
        {
            throw new NotSupportedException("Cannot set the value of a method Member.");
        }

        public override object GetValue(object target)
        {
            return _member.Invoke(target, null);
        }

        public override bool TryGetBackingField(out Member field)
        {
            if (_backingField != null)
            {
                field = _backingField;
                return true;
            }

            var name = Name;

            if (name.StartsWith("Get", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(3);
            }

            var reflectedField = DeclaringType.GetField(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            reflectedField = reflectedField ?? DeclaringType.GetField("_" + name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            reflectedField = reflectedField ?? DeclaringType.GetField("m_" + name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (reflectedField == null)
            {
                field = null;
                return false;
            }

            field = _backingField = new FieldMember(reflectedField);
            return true;
        }

        public MethodMember(MethodInfo member)
        {
            _member = member;
        }

        public override string Name
        {
            get 
            { 
                return _member.Name; 
            }
        }

        public override Type PropertyType
        {
            get 
            { 
                return _member.ReturnType; 
            }
        }
        public override bool CanWrite
        {
            get 
            { 
                return false; 
            }
        }
        public override MemberInfo MemberInfo
        {
            get 
            { 
                return _member; 
            }
        }
        public override Type DeclaringType
        {
            get 
            { 
                return _member.DeclaringType; 
            }
        }
        public override bool HasIndexParameters
        {
            get 
            { 
                return false; 
            }
        }
        public override bool IsMethod
        {
            get 
            { 
                return true; 
            }
        }
        public override bool IsField
        {
            get 
            { 
                return false; 
            }
        }
        public override bool IsProperty
        {
            get 
            { 
                return false; 
            }
        }

        public override bool IsAutoProperty
        {
            get 
            { 
                return false; 
            }
        }

        public override bool IsPrivate
        {
            get 
            { 
                return _member.IsPrivate; 
            }
        }

        public override bool IsProtected
        {
            get 
            { 
                return _member.IsFamily || _member.IsFamilyAndAssembly; 
            }
        }

        public override bool IsPublic
        {
            get 
            { 
                return _member.IsPublic; 
            }
        }

        public override bool IsInternal
        {
            get 
            { 
                return _member.IsAssembly || _member.IsFamilyAndAssembly; 
            }
        }

        public bool IsCompilerGenerated
        {
            get 
            { 
                return _member.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true)
                              .Any(); 
            }
        }

        public override string ToString()
        {
            return "{Method: " + _member.Name + "}";
        }
    }

    [Serializable]
    internal class FieldMember: Member
    {
        private readonly FieldInfo _member;

        public override void SetValue(object target, object value)
        {
            _member.SetValue(target, value);
        }

        public override object GetValue(object target)
        {
            return _member.GetValue(target);
        }

        public override bool TryGetBackingField(out Member backingField)
        {
            backingField = null;
            return false;
        }

        public FieldMember(FieldInfo member)
        {
            _member = member;
        }

        public override string Name
        {
            get 
            { 
                return _member.Name; 
            }
        }
        public override Type PropertyType
        {
            get 
            { 
                return _member.FieldType; 
            }
        }
        public override bool CanWrite
        {
            get 
            { 
                return true; 
            }
        }
        public override MemberInfo MemberInfo
        {
            get 
            { 
                return _member; 
            }
        }
        public override Type DeclaringType
        {
            get 
            { 
                return _member.DeclaringType; 
            }
        }
        public override bool HasIndexParameters
        {
            get 
            { 
                return false; 
            }
        }
        public override bool IsMethod
        {
            get 
            { 
                return false; 
            }
        }
        public override bool IsField
        {
            get 
            { 
                return true; 
            }
        }
        public override bool IsProperty
        {
            get 
            { 
                return false; 
            }
        }

        public override bool IsAutoProperty
        {
            get 
            { 
                return false; 
            }
        }

        public override bool IsPrivate
        {
            get 
            { 
                return _member.IsPrivate; 
            }
        }

        public override bool IsProtected
        {
            get 
            { 
                return _member.IsFamily || _member.IsFamilyAndAssembly; 
            }
        }

        public override bool IsPublic
        {
            get 
            { 
                return _member.IsPublic; 
            }
        }

        public override bool IsInternal
        {
            get 
            { 
                return _member.IsAssembly || _member.IsFamilyAndAssembly; 
            }
        }

        public override string ToString()
        {
            return "{Field: " + _member.Name + "}";
        }
    }

    [Serializable]
    internal class PropertyMember: Member
    {
        private readonly PropertyInfo _member;
        private readonly MethodMember _getMethod;
        private readonly MethodMember _setMethod;
        private Member _backingField;

        public PropertyMember(PropertyInfo member)
        {
            _member = member;
            _getMethod = GetMember(member.GetGetMethod(true));
            _setMethod = GetMember(member.GetSetMethod(true));
        }

        MethodMember GetMember(MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }

            return (MethodMember)method.ToMember();
        }

        public override void SetValue(object target, object value)
        {
            _member.SetValue(target, value, null);
        }

        public override object GetValue(object target)
        {
            return _member.GetValue(target, null);
        }

        public override bool TryGetBackingField(out Member field)
        {
            if (_backingField != null)
            {
                field = _backingField;
                return true;
            }

            var reflectedField = DeclaringType.GetField(Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            reflectedField = reflectedField ?? DeclaringType.GetField("_" + Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            reflectedField = reflectedField ?? DeclaringType.GetField("m_" + Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (reflectedField == null)
            {
                field = null;
                return false;
            }

            field = _backingField = new FieldMember(reflectedField);
            return true;
        }

        public override string Name
        {
            get 
            { 
                return _member.Name; 
            }
        }
        public override Type PropertyType
        {
            get 
            { 
                return _member.PropertyType; 
            }
        }
        public override bool CanWrite
        {
            get
            {
                // override the default reflection value here. Private setters aren't
                // considered "settable" in the same sense that public ones are. We can
                // use this to control the access strategy later
                if (IsAutoProperty && (_setMethod == null || _setMethod.IsPrivate))
                {
                    return false;
                }

                return _member.CanWrite;
            }
        }
        public override MemberInfo MemberInfo
        {
            get 
            { 
                return _member; 
            }
        }
        public override Type DeclaringType
        {
            get 
            { 
                return _member.DeclaringType; 
            }
        }
        public override bool HasIndexParameters
        {
            get 
            { 
                return _member.GetIndexParameters().Length > 0; 
            }
        }
        public override bool IsMethod
        {
            get 
            { 
                return false; 
            }
        }
        public override bool IsField
        {
            get 
            { 
                return false; 
            }
        }
        public override bool IsProperty
        {
            get 
            { 
                return true; 
            }
        }

        public override bool IsAutoProperty
        {
            get
            {
                return (_getMethod != null && _getMethod.IsCompilerGenerated)
                    || (_setMethod != null && _setMethod.IsCompilerGenerated);
            }
        }

        public override bool IsPrivate
        {
            get 
            { 
                return _getMethod.IsPrivate; 
            }
        }

        public override bool IsProtected
        {
            get 
            { 
                return _getMethod.IsProtected; 
            }
        }

        public override bool IsPublic
        {
            get 
            { 
                return _getMethod.IsPublic; 
            }
        }

        public override bool IsInternal
        {
            get 
            { 
                return _getMethod.IsInternal; 
            }
        }

        public MethodMember Get
        {
            get 
            { 
                return _getMethod; 
            }
        }

        public MethodMember Set
        {
            get 
            { 
                return _setMethod; 
            }
        }

        public override string ToString()
        {
            return "{Property: " + _member.Name + "}";
        }
    }

    public static class MemberExtensions
    {
        public static Member ToMember(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new NullReferenceException("Cannot create member from null.");
            }

            return new PropertyMember(propertyInfo);
        }

        public static Member ToMember(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException("Cannot create member from null.");
            }

            return new MethodMember(methodInfo);
        }

        public static Member ToMember(this FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new NullReferenceException("Cannot create member from null.");
            }

            return new FieldMember(fieldInfo);
        }

        public static Member ToMember(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new NullReferenceException("Cannot create member from null.");
            }

            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo)memberInfo).ToMember();
            }

            if (memberInfo is FieldInfo)
            {
                return ((FieldInfo)memberInfo).ToMember();
            }

            if (memberInfo is MethodInfo)
            {
                return ((MethodInfo)memberInfo).ToMember();
            }

            throw new InvalidOperationException("Cannot convert MemberInfo '" + memberInfo.Name + "' to Member.");
        }

        public static IEnumerable<Member> GetInstanceFields(this Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.Name.StartsWith("<"))
                {
                    yield return field.ToMember();
                }
            }
        }

        public static IEnumerable<Member> GetInstanceMethods(this Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && method.ReturnType != typeof(void) && method.GetParameters().Length == 0)
                {
                    yield return method.ToMember();
                }
            }
        }

        public static IEnumerable<Member> GetInstanceProperties(this Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                yield return property.ToMember();
            }
        }

        public static IEnumerable<Member> GetInstanceMembers(this Type type)
        {
            var members = new HashSet<Member>(new MemberEqualityComparer());

            type.GetInstanceProperties()
                .ForEach(x => members.Add(x));

            type.GetInstanceFields()
                .ForEach(x => members.Add(x));

            type.GetInstanceMethods()
                .ForEach(x => members.Add(x));

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                type.BaseType.GetInstanceMembers()
                             .ForEach(x => members.Add(x));
            }

            return members;
        }
    }

    public class MemberEqualityComparer: IEqualityComparer<Member>
    {
        public bool Equals(Member x, Member y)
        {
            return x.MemberInfo.MetadataToken.Equals(y.MemberInfo.MetadataToken) && x.MemberInfo.Module.Equals(y.MemberInfo.Module);
        }

        public int GetHashCode(Member obj)
        {
            return obj.MemberInfo.MetadataToken.GetHashCode() & obj.MemberInfo.Module.GetHashCode();
        }
    }
}
