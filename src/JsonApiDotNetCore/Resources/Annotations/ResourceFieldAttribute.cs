using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API field (attribute or relationship). See
    /// https://jsonapi.org/format/#document-resource-object-fields.
    /// </summary>
    [PublicAPI]
    public abstract class ResourceFieldAttribute : Attribute
    {
        // These are definitely assigned after building the resource graph, which is why their public equivalents are declared as non-nullable.
        private string? _publicName;
        private PropertyInfo? _property;

        /// <summary>
        /// The publicly exposed name of this JSON:API field. When not explicitly assigned, the configured naming convention is applied on the property name.
        /// </summary>
        public string PublicName
        {
            get => _publicName!;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Exposed name cannot be null, empty or contain only whitespace.", nameof(value));
                }

                _publicName = value;
            }
        }

        /// <summary>
        /// The resource property that this attribute is declared on.
        /// </summary>
        public PropertyInfo Property
        {
            get => _property!;
            internal set
            {
                ArgumentGuard.NotNull(value, nameof(value));
                _property = value;
            }
        }

        /// <summary>
        /// Gets the value of this field on the specified resource instance. Throws if the property is write-only or if the field does not belong to the
        /// specified resource instance.
        /// </summary>
        public object? GetValue(object resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            if (Property.GetMethod == null)
            {
                throw new InvalidOperationException($"Property '{Property.DeclaringType?.Name}.{Property.Name}' is write-only.");
            }

            try
            {
                return Property.GetValue(resource);
            }
            catch (TargetException exception)
            {
                throw new InvalidOperationException(
                    $"Unable to get property value of '{Property.DeclaringType!.Name}.{Property.Name}' on instance of type '{resource.GetType().Name}'.",
                    exception);
            }
        }

        /// <summary>
        /// Sets the value of this field on the specified resource instance. Throws if the property is read-only or if the field does not belong to the specified
        /// resource instance.
        /// </summary>
        public void SetValue(object resource, object? newValue)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            if (Property.SetMethod == null)
            {
                throw new InvalidOperationException($"Property '{Property.DeclaringType?.Name}.{Property.Name}' is read-only.");
            }

            try
            {
                Property.SetValue(resource, newValue);
            }
            catch (TargetException exception)
            {
                throw new InvalidOperationException(
                    $"Unable to set property value of '{Property.DeclaringType!.Name}.{Property.Name}' on instance of type '{resource.GetType().Name}'.",
                    exception);
            }
        }

        public override string? ToString()
        {
            return _publicName ?? (_property != null ? _property.Name : base.ToString());
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

            var other = (ResourceFieldAttribute)obj;

            return _publicName == other._publicName && _property == other._property;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_publicName, _property);
        }
    }
}
