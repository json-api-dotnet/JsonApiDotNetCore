using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.OpenApi.Client;

/// <summary>
/// Base class to inherit auto-generated OpenAPI clients from. Provides support for partial POST/PATCH in JSON:API requests.
/// </summary>
public abstract class JsonApiClient : IJsonApiClient
{
    private readonly DocumentJsonConverter _documentJsonConverter = new();

    /// <summary>
    /// Initial setup. Call this from the UpdateJsonSerializerSettings partial method in the auto-generated OpenAPI client.
    /// </summary>
    protected void SetSerializerSettingsForJsonApi(JsonSerializerSettings settings)
    {
        ArgumentGuard.NotNull(settings);

        settings.Converters.Add(_documentJsonConverter);
    }

    /// <inheritdoc />
    public IDisposable WithPartialAttributeSerialization<TRequestDocument, TAttributesObject>(TRequestDocument requestDocument,
        params Expression<Func<TAttributesObject, object?>>[] alwaysIncludedAttributeSelectors)
        where TRequestDocument : class
    {
        ArgumentGuard.NotNull(requestDocument);

        var attributeNames = new HashSet<string>();

        foreach (Expression<Func<TAttributesObject, object?>> selector in alwaysIncludedAttributeSelectors)
        {
            if (RemoveConvert(selector.Body) is MemberExpression selectorBody)
            {
                attributeNames.Add(selectorBody.Member.Name);
            }
            else
            {
                throw new ArgumentException($"The expression '{selector}' should select a single property. For example: 'article => article.Title'.",
                    nameof(alwaysIncludedAttributeSelectors));
            }
        }

        var alwaysIncludedAttributes = new AlwaysIncludedAttributes(attributeNames, typeof(TAttributesObject));
        _documentJsonConverter.RegisterDocument(requestDocument, alwaysIncludedAttributes);

        return new DocumentRegistrationScope(_documentJsonConverter, requestDocument);
    }

    private static Expression RemoveConvert(Expression expression)
    {
        Expression innerExpression = expression;

        while (true)
        {
            if (innerExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
            {
                innerExpression = unaryExpression.Operand;
            }
            else
            {
                return innerExpression;
            }
        }
    }

    /// <summary>
    /// Tracks a JSON:API attributes registration for a JSON:API document instance in the serializer. Disposing removes the registration, so the client can
    /// be reused.
    /// </summary>
    private sealed class DocumentRegistrationScope : IDisposable
    {
        private readonly DocumentJsonConverter _documentJsonConverter;
        private readonly object _document;

        public DocumentRegistrationScope(DocumentJsonConverter documentJsonConverter, object document)
        {
            ArgumentGuard.NotNull(documentJsonConverter);
            ArgumentGuard.NotNull(document);

            _documentJsonConverter = documentJsonConverter;
            _document = document;
        }

        public void Dispose()
        {
            _documentJsonConverter.UnRegisterDocument(_document);
        }
    }

    /// <summary>
    /// Represents the set of JSON:API attributes to always send to the server, even if they are uninitialized (contain default value).
    /// </summary>
    private sealed class AlwaysIncludedAttributes
    {
        private readonly ISet<string> _propertyNames;
        private readonly Type _attributesObjectType;

        public AlwaysIncludedAttributes(ISet<string> propertyNames, Type attributesObjectType)
        {
            ArgumentGuard.NotNull(propertyNames);
            ArgumentGuard.NotNull(attributesObjectType);

            _propertyNames = propertyNames;
            _attributesObjectType = attributesObjectType;
        }

        public bool ContainsAttribute(string propertyName)
        {
            return _propertyNames.Contains(propertyName);
        }

        public bool IsAttributesObjectType(Type type)
        {
            return _attributesObjectType == type;
        }
    }

    /// <summary>
    /// A <see cref="JsonConverter" /> that acts on JSON:API documents.
    /// </summary>
    private sealed class DocumentJsonConverter : JsonConverter
    {
        private readonly Dictionary<object, AlwaysIncludedAttributes> _alwaysIncludedAttributesByDocument = new();
        private readonly Dictionary<Type, ISet<object>> _documentsByType = new();
        private bool _isSerializing;

        public override bool CanRead => false;

        public void RegisterDocument(object document, AlwaysIncludedAttributes alwaysIncludedAttributes)
        {
            _alwaysIncludedAttributesByDocument[document] = alwaysIncludedAttributes;

            Type documentType = document.GetType();

            if (!_documentsByType.ContainsKey(documentType))
            {
                _documentsByType[documentType] = new HashSet<object>();
            }

            _documentsByType[documentType].Add(document);
        }

        public void UnRegisterDocument(object document)
        {
            if (_alwaysIncludedAttributesByDocument.ContainsKey(document))
            {
                _alwaysIncludedAttributesByDocument.Remove(document);

                Type documentType = document.GetType();
                _documentsByType[documentType].Remove(document);

                if (!_documentsByType[documentType].Any())
                {
                    _documentsByType.Remove(documentType);
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            ArgumentGuard.NotNull(objectType);

            if (_isSerializing)
            {
                // Protect against infinite recursion.
                return false;
            }

            return _documentsByType.ContainsKey(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new UnreachableCodeException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ArgumentGuard.NotNull(writer);
            ArgumentGuard.NotNull(serializer);

            if (value != null)
            {
                if (_alwaysIncludedAttributesByDocument.TryGetValue(value, out AlwaysIncludedAttributes? alwaysIncludedAttributes))
                {
                    var attributesJsonConverter = new AttributesJsonConverter(alwaysIncludedAttributes);
                    serializer.Converters.Add(attributesJsonConverter);
                }

                try
                {
                    _isSerializing = true;
                    serializer.Serialize(writer, value);
                }
                finally
                {
                    _isSerializing = false;
                }
            }
        }
    }

    /// <summary>
    /// A <see cref="JsonConverter" /> that acts on JSON:API attribute objects.
    /// </summary>
    private sealed class AttributesJsonConverter : JsonConverter
    {
        private readonly AlwaysIncludedAttributes _alwaysIncludedAttributes;
        private bool _isSerializing;

        public override bool CanRead => false;

        public AttributesJsonConverter(AlwaysIncludedAttributes alwaysIncludedAttributes)
        {
            ArgumentGuard.NotNull(alwaysIncludedAttributes);

            _alwaysIncludedAttributes = alwaysIncludedAttributes;
        }

        public override bool CanConvert(Type objectType)
        {
            ArgumentGuard.NotNull(objectType);

            if (_isSerializing)
            {
                // Protect against infinite recursion.
                return false;
            }

            return _alwaysIncludedAttributes.IsAttributesObjectType(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new UnreachableCodeException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ArgumentGuard.NotNull(writer);
            ArgumentGuard.NotNull(serializer);

            if (value != null)
            {
                if (_alwaysIncludedAttributes.IsAttributesObjectType(value.GetType()))
                {
                    AssertRequiredAttributesHaveNonDefaultValues(value, writer.Path);

                    serializer.ContractResolver = new JsonApiAttributeContractResolver(_alwaysIncludedAttributes);
                }

                try
                {
                    _isSerializing = true;
                    serializer.Serialize(writer, value);
                }
                finally
                {
                    _isSerializing = false;
                }
            }
        }

        private void AssertRequiredAttributesHaveNonDefaultValues(object attributesObject, string jsonPath)
        {
            foreach (PropertyInfo propertyInfo in attributesObject.GetType().GetProperties())
            {
                bool isExplicitlyIncluded = _alwaysIncludedAttributes.ContainsAttribute(propertyInfo.Name);

                if (!isExplicitlyIncluded)
                {
                    AssertPropertyHasNonDefaultValueIfRequired(attributesObject, propertyInfo, jsonPath);
                }
            }
        }

        private static void AssertPropertyHasNonDefaultValueIfRequired(object attributesObject, PropertyInfo propertyInfo, string jsonPath)
        {
            var jsonProperty = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();

            if (jsonProperty is { Required: Required.Always or Required.AllowNull })
            {
                bool propertyHasDefaultValue = PropertyHasDefaultValue(propertyInfo, attributesObject);

                if (propertyHasDefaultValue)
                {
                    throw new InvalidOperationException(
                        $"Required property '{propertyInfo.Name}' at JSON path '{jsonPath}.{jsonProperty.PropertyName}' is not set. If sending its default value is intended, include it explicitly.");
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
    /// Corrects the <see cref="NullValueHandling" /> and <see cref="DefaultValueHandling" /> JSON annotations at runtime, which appear on the auto-generated
    /// properties for JSON:API attributes. For example:
    /// <code><![CDATA[
    /// [Newtonsoft.Json.JsonProperty("firstName", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    /// ]]>
    /// </code>
    /// </summary>
    private sealed class JsonApiAttributeContractResolver : DefaultContractResolver
    {
        private readonly AlwaysIncludedAttributes _alwaysIncludedAttributes;

        public JsonApiAttributeContractResolver(AlwaysIncludedAttributes alwaysIncludedAttributes)
        {
            ArgumentGuard.NotNull(alwaysIncludedAttributes);

            _alwaysIncludedAttributes = alwaysIncludedAttributes;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);

            if (_alwaysIncludedAttributes.IsAttributesObjectType(jsonProperty.DeclaringType!))
            {
                if (_alwaysIncludedAttributes.ContainsAttribute(jsonProperty.UnderlyingName!))
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
}
