using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.OpenApi.Client.NSwag;

/// <summary>
/// Base class to inherit auto-generated NSwag OpenAPI clients from. Provides support for partial POST/PATCH in JSON:API requests, optionally combined
/// with OpenAPI inheritance.
/// </summary>
[PublicAPI]
public abstract class JsonApiClient
{
    private const string GeneratedJsonInheritanceConverterName = "JsonInheritanceConverter";
    private static readonly DefaultContractResolver UnmodifiedContractResolver = new();

    private readonly Dictionary<INotifyPropertyChanged, ISet<string>> _propertyStore = [];

    /// <summary>
    /// Whether to automatically clear tracked properties after sending a request. Default value: <c>true</c>. Set to <c>false</c> to reuse tracked
    /// properties for multiple requests and call <see cref="ClearAllTracked" /> after the last request to clean up.
    /// </summary>
    public bool AutoClearTracked { get; set; } = true;

    internal void Track<T>(T container)
        where T : INotifyPropertyChanged, new()
    {
        container.PropertyChanged += ContainerOnPropertyChanged;

        MarkAsTracked(container);
    }

    private void ContainerOnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is INotifyPropertyChanged container && args.PropertyName != null)
        {
            MarkAsTracked(container, args.PropertyName);
        }
    }

    /// <summary>
    /// Marks the specified properties on an object instance as tracked. Use this when unable to use inline initializer syntax for tracking.
    /// </summary>
    /// <param name="container">
    /// The object instance whose properties to mark as tracked.
    /// </param>
    /// <param name="propertyNames">
    /// The names of the properties to mark as tracked. Properties in this list are always included. Any other property is only included if its value differs
    /// from the property type's default value.
    /// </param>
    public void MarkAsTracked(INotifyPropertyChanged container, params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(propertyNames);

        if (!_propertyStore.TryGetValue(container, out ISet<string>? properties))
        {
            properties = new HashSet<string>();
            _propertyStore[container] = properties;
        }

        foreach (string propertyName in propertyNames)
        {
            properties.Add(propertyName);
        }
    }

    /// <summary>
    /// Clears all tracked properties. Call this after sending multiple requests when <see cref="AutoClearTracked" /> is set to <c>false</c>.
    /// </summary>
    public void ClearAllTracked()
    {
        foreach (INotifyPropertyChanged container in _propertyStore.Keys)
        {
            container.PropertyChanged -= ContainerOnPropertyChanged;
        }

        _propertyStore.Clear();
    }

    private void RemoveContainer(INotifyPropertyChanged container)
    {
        container.PropertyChanged -= ContainerOnPropertyChanged;
        _propertyStore.Remove(container);
    }

    /// <summary>
    /// Initial setup. Call this from the Initialize partial method in the auto-generated NSwag client.
    /// </summary>
    /// <param name="serializerSettings">
    /// The <see cref="JsonSerializerSettings" /> to configure.
    /// </param>
    /// <remarks>
    /// CAUTION: Calling this method makes the serializer stateful, which removes thread-safety of the owning auto-generated NSwag client. As a result, the
    /// client MUST NOT be shared. So don't use a static instance, and don't register as a singleton in the service container. Also, do not execute parallel
    /// requests on the same NSwag client instance. Executing multiple sequential requests on the same generated client instance is fine.
    /// </remarks>
    protected void SetSerializerSettingsForJsonApi(JsonSerializerSettings serializerSettings)
    {
        ArgumentNullException.ThrowIfNull(serializerSettings);

        serializerSettings.ContractResolver = new InsertDiscriminatorPropertyContractResolver();
        serializerSettings.Converters.Insert(0, new PropertyTrackingInheritanceConverter(this));
    }

    private static string? GetDiscriminatorName(Type objectType)
    {
        JsonContract contract = UnmodifiedContractResolver.ResolveContract(objectType);

        if (contract.Converter != null && contract.Converter.GetType().Name == GeneratedJsonInheritanceConverterName)
        {
            var inheritanceConverter = (BlockedJsonInheritanceConverter)contract.Converter;
            return inheritanceConverter.DiscriminatorName;
        }

        return null;
    }

    /// <summary>
    /// Replacement for the writing part of client-generated JsonInheritanceConverter that doesn't block other converters and preserves the JSON path on
    /// error.
    /// </summary>
    private class InsertDiscriminatorPropertyContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            // NSwag adds [JsonConverter(typeof(JsonInheritanceConverter), "type")] on types to write the discriminator.
            // This annotation has higher precedence over converters in the serializer settings, which is why ours normally won't execute.
            // Once we tell Newtonsoft to ignore JsonInheritanceConverter, our converter can kick in.

            JsonObjectContract contract = base.CreateObjectContract(objectType);

            if (contract.Converter != null && contract.Converter.GetType().Name == GeneratedJsonInheritanceConverterName)
            {
                contract.Converter = null;
            }

            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            string? discriminatorName = GetDiscriminatorName(type);

            if (discriminatorName != null)
            {
                JsonProperty discriminatorProperty = CreateDiscriminatorProperty(discriminatorName, type);
                properties.Insert(0, discriminatorProperty);
            }

            return properties;
        }

        private static JsonProperty CreateDiscriminatorProperty(string discriminatorName, Type declaringType)
        {
            return new JsonProperty
            {
                PropertyName = discriminatorName,
                PropertyType = typeof(string),
                DeclaringType = declaringType,
                ValueProvider = new DiscriminatorValueProvider(),
                Readable = true,
                Writable = true
            };
        }

        private sealed class DiscriminatorValueProvider : IValueProvider
        {
            public object? GetValue(object target)
            {
                Type type = target.GetType();

                foreach (Attribute attribute in type.GetCustomAttributes<Attribute>(true))
                {
                    var shim = JsonInheritanceAttributeShim.TryCreate(attribute);

                    if (shim != null && shim.Type == type)
                    {
                        return shim.Key;
                    }
                }

                return null;
            }

            public void SetValue(object target, object? value)
            {
                // Nothing to do, NSwag doesn't generate a property for the discriminator.
            }
        }
    }

    /// <summary>
    /// Provides support for writing partial POST/PATCH in JSON:API requests via tracked properties. Provides reading of discriminator for inheritance.
    /// </summary>
    private sealed class PropertyTrackingInheritanceConverter : JsonConverter
    {
        [ThreadStatic]
        private static bool _isWriting;

        [ThreadStatic]
        private static bool _isReading;

        private readonly JsonApiClient _apiClient;

        public override bool CanRead
        {
            get
            {
                if (_isReading)
                {
                    // Prevent infinite recursion, but auto-reset so we'll participate in nested objects.
                    _isReading = false;
                    return false;
                }

                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_isWriting)
                {
                    // Prevent infinite recursion, but auto-reset so we'll participate in nested objects.
                    _isWriting = false;
                    return false;
                }

                return true;
            }
        }

        public PropertyTrackingInheritanceConverter(JsonApiClient apiClient)
        {
            ArgumentNullException.ThrowIfNull(apiClient);

            _apiClient = apiClient;
        }

        public override bool CanConvert(Type objectType)
        {
            // Because this is called BEFORE CanRead/CanWrite, respond to both tracking and inheritance.
            // We don't actually write for inheritance, so bail out later if that's the case.

            if (_apiClient._propertyStore.Keys.Any(containingType => containingType.GetType() == objectType))
            {
                return true;
            }

            var converterAttribute = objectType.GetCustomAttribute<JsonConverterAttribute>(true);
            return converterAttribute is { ConverterType.Name: GeneratedJsonInheritanceConverterName };
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            _isReading = true;

            try
            {
                JToken token = JToken.ReadFrom(reader);
                string? discriminatorValue = GetDiscriminatorValue(objectType, token);

                Type resolvedType = ResolveTypeFromDiscriminatorValue(objectType, discriminatorValue);
                return token.ToObject(resolvedType, serializer);
            }
            finally
            {
                _isReading = false;
            }
        }

        private static string? GetDiscriminatorValue(Type objectType, JToken token)
        {
            var jsonConverterAttribute = objectType.GetCustomAttribute<JsonConverterAttribute>(true)!;

            if (jsonConverterAttribute.ConverterParameters is not [string])
            {
                throw new JsonException($"Expected single 'type' parameter for JsonInheritanceConverter usage on type '{objectType}'.");
            }

            string discriminatorName = (string)jsonConverterAttribute.ConverterParameters[0];
            return token.Children<JProperty>().FirstOrDefault(property => property.Name == discriminatorName)?.Value.ToString();
        }

        private static Type ResolveTypeFromDiscriminatorValue(Type objectType, string? discriminatorValue)
        {
            if (discriminatorValue != null)
            {
                foreach (Attribute attribute in objectType.GetCustomAttributes<Attribute>(true))
                {
                    var shim = JsonInheritanceAttributeShim.TryCreate(attribute);

                    if (shim != null && shim.Key == discriminatorValue)
                    {
                        return shim.Type;
                    }
                }
            }

            return objectType;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            _isWriting = true;

            try
            {
                if (value is INotifyPropertyChanged container && _apiClient._propertyStore.TryGetValue(container, out ISet<string>? properties))
                {
                    // Because we're overwriting NullValueHandling/DefaultValueHandling, we miss out on some validations that Newtonsoft would otherwise run.
                    AssertRequiredTrackedPropertiesHaveNoDefaultValue(container, properties, writer.Path);

                    IContractResolver backupContractResolver = serializer.ContractResolver;

                    try
                    {
                        // Caution: Swapping the contract resolver is not safe for concurrent usage, yet it needs to know the tracked instance.
                        serializer.ContractResolver = new PropertyTrackingContractResolver(container, properties);
                        serializer.Serialize(writer, value);

                        if (_apiClient.AutoClearTracked)
                        {
                            _apiClient.RemoveContainer(container);
                        }
                    }
                    finally
                    {
                        serializer.ContractResolver = backupContractResolver;
                    }
                }
                else
                {
                    // We get here when the type is tracked, but not this instance. Or when writing for inheritance.
                    serializer.Serialize(writer, value);
                }
            }
            finally
            {
                _isWriting = false;
            }
        }

        private static void AssertRequiredTrackedPropertiesHaveNoDefaultValue(object container, ISet<string> properties, string jsonPath)
        {
            foreach (PropertyInfo propertyInfo in container.GetType().GetProperties())
            {
                bool isTracked = properties.Contains(propertyInfo.Name);

                if (!isTracked)
                {
                    AssertPropertyHasNonDefaultValueIfRequired(container, propertyInfo, jsonPath);
                }
            }
        }

        private static void AssertPropertyHasNonDefaultValueIfRequired(object attributesObject, PropertyInfo propertyInfo, string jsonPath)
        {
            var jsonProperty = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();

            if (jsonProperty is { Required: Required.Always or Required.AllowNull })
            {
                if (PropertyHasDefaultValue(propertyInfo, attributesObject))
                {
                    throw new JsonSerializationException(
                        $"Cannot write a default value for property '{jsonProperty.PropertyName}'. Property requires a non-default value. Path '{jsonPath}'.",
                        jsonPath, 0, 0, null);
                }
            }
        }

        private static bool PropertyHasDefaultValue(PropertyInfo propertyInfo, object instance)
        {
            object? propertyValue = propertyInfo.GetValue(instance);
            object? defaultValue = GetDefaultValue(propertyInfo.PropertyType);

            return EqualityComparer<object>.Default.Equals(propertyValue, defaultValue);
        }

        private static object? GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }

    /// <summary>
    /// Overrules the <see cref="NullValueHandling" /> and <see cref="DefaultValueHandling" /> annotations on generated properties for tracked object
    /// instances to support JSON:API partial POST/PATCH.
    /// </summary>
    private sealed class PropertyTrackingContractResolver : InsertDiscriminatorPropertyContractResolver
    {
        private readonly INotifyPropertyChanged _container;
        private readonly ISet<string> _properties;

        public PropertyTrackingContractResolver(INotifyPropertyChanged container, ISet<string> properties)
        {
            ArgumentNullException.ThrowIfNull(container);
            ArgumentNullException.ThrowIfNull(properties);

            _container = container;
            _properties = properties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);

            if (jsonProperty.DeclaringType == _container.GetType())
            {
                if (_properties.Contains(jsonProperty.UnderlyingName!))
                {
                    jsonProperty.NullValueHandling = NullValueHandling.Include;
                    jsonProperty.DefaultValueHandling = DefaultValueHandling.Include;
                }
                else
                {
                    jsonProperty.NullValueHandling = NullValueHandling.Ignore;
                    jsonProperty.DefaultValueHandling = DefaultValueHandling.Ignore;
                }
            }

            return jsonProperty;
        }
    }

    private sealed class JsonInheritanceAttributeShim
    {
        private readonly Attribute _instance;
        private readonly PropertyInfo _keyProperty;
        private readonly PropertyInfo _typeProperty;

        public string Key => (string)_keyProperty.GetValue(_instance)!;
        public Type Type => (Type)_typeProperty.GetValue(_instance)!;

        private JsonInheritanceAttributeShim(Attribute instance, Type type)
        {
            _instance = instance;
            _keyProperty = type.GetProperty("Key") ?? throw new ArgumentException("Key property not found.", nameof(instance));
            _typeProperty = type.GetProperty("Type") ?? throw new ArgumentException("Type property not found.", nameof(instance));
        }

        public static JsonInheritanceAttributeShim? TryCreate(Attribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            Type type = attribute.GetType();
            return type.Name == "JsonInheritanceAttribute" ? new JsonInheritanceAttributeShim(attribute, type) : null;
        }
    }
}
