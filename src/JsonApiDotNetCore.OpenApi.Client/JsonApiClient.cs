using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace JsonApiDotNetCore.OpenApi.Client;

/// <summary>
/// Base class to inherit auto-generated client from. Enables to mark fields to be explicitly included in a request body, even if they are null or
/// default.
/// </summary>
[PublicAPI]
public abstract class JsonApiClient : IJsonApiClient
{
    private readonly JsonApiJsonConverter _jsonApiJsonConverter = new();

    protected void SetSerializerSettingsForJsonApi(JsonSerializerSettings settings)
    {
        ArgumentGuard.NotNull(settings);

        settings.Converters.Add(_jsonApiJsonConverter);
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
                throw new ArgumentException(
                    $"The expression '{nameof(alwaysIncludedAttributeSelectors)}' should select a single property. For example: 'article => article.Title'.");
            }
        }

        _jsonApiJsonConverter.RegisterDocument(requestDocument, new AttributeNamesContainer(attributeNames, typeof(TAttributesObject)));

        return new RequestDocumentRegistrationScope(_jsonApiJsonConverter, requestDocument);
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

    private sealed class JsonApiJsonConverter : JsonConverter
    {
        private readonly Dictionary<object, AttributeNamesContainer> _alwaysIncludedAttributesByRequestDocument = new();
        private readonly Dictionary<Type, ISet<object>> _requestDocumentsByType = new();
        private SerializationScope? _serializationScope;

        public override bool CanRead => false;

        public void RegisterDocument(object requestDocument, AttributeNamesContainer alwaysIncludedAttributes)
        {
            _alwaysIncludedAttributesByRequestDocument[requestDocument] = alwaysIncludedAttributes;

            Type requestDocumentType = requestDocument.GetType();

            if (!_requestDocumentsByType.ContainsKey(requestDocumentType))
            {
                _requestDocumentsByType[requestDocumentType] = new HashSet<object>();
            }

            _requestDocumentsByType[requestDocumentType].Add(requestDocument);
        }

        public void RemoveRegistration(object requestDocument)
        {
            if (_alwaysIncludedAttributesByRequestDocument.ContainsKey(requestDocument))
            {
                _alwaysIncludedAttributesByRequestDocument.Remove(requestDocument);

                Type requestDocumentType = requestDocument.GetType();
                _requestDocumentsByType[requestDocumentType].Remove(requestDocument);

                if (!_requestDocumentsByType[requestDocumentType].Any())
                {
                    _requestDocumentsByType.Remove(requestDocumentType);
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            ArgumentGuard.NotNull(objectType);

            if (_serializationScope == null)
            {
                return _requestDocumentsByType.ContainsKey(objectType);
            }

            return _serializationScope.ShouldConvertAsAttributesObject(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new UnreachableCodeException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ArgumentGuard.NotNull(writer);
            ArgumentGuard.NotNull(value);
            ArgumentGuard.NotNull(serializer);

            if (_serializationScope == null)
            {
                AssertObjectIsRequestDocument(value);

                SerializeRequestDocument(writer, value, serializer);
            }
            else
            {
                AttributeNamesContainer? attributesObjectInfo = _serializationScope.AttributesObjectInScope;

                AssertObjectMatchesSerializationScope(attributesObjectInfo, value);

                SerializeAttributesObject(attributesObjectInfo, writer, value, serializer);
            }
        }

        private void AssertObjectIsRequestDocument(object value)
        {
            Type objectType = value.GetType();

            if (!_requestDocumentsByType.ContainsKey(objectType))
            {
                throw new UnreachableCodeException();
            }
        }

        private void SerializeRequestDocument(JsonWriter writer, object value, JsonSerializer serializer)
        {
            _serializationScope = new SerializationScope();

            if (_alwaysIncludedAttributesByRequestDocument.TryGetValue(value, out AttributeNamesContainer? attributesObjectInfo))
            {
                _serializationScope.AttributesObjectInScope = attributesObjectInfo;
            }

            try
            {
                serializer.Serialize(writer, value);
            }
            finally
            {
                _serializationScope = null;
            }
        }

        private static void AssertObjectMatchesSerializationScope([SysNotNull] AttributeNamesContainer? attributesObjectInfo, object value)
        {
            Type objectType = value.GetType();

            if (attributesObjectInfo == null || !attributesObjectInfo.MatchesAttributesObjectType(objectType))
            {
                throw new UnreachableCodeException();
            }
        }

        private static void SerializeAttributesObject(AttributeNamesContainer alwaysIncludedAttributes, JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            AssertRequiredPropertiesAreNotExcluded(value, alwaysIncludedAttributes, writer);

            serializer.ContractResolver = new JsonApiAttributeContractResolver(alwaysIncludedAttributes);
            serializer.Serialize(writer, value);
        }

        private static void AssertRequiredPropertiesAreNotExcluded(object value, AttributeNamesContainer alwaysIncludedAttributes, JsonWriter jsonWriter)
        {
            PropertyInfo[] propertyInfos = value.GetType().GetProperties();

            foreach (PropertyInfo attributesPropertyInfo in propertyInfos)
            {
                bool isExplicitlyIncluded = alwaysIncludedAttributes.ContainsAttribute(attributesPropertyInfo.Name);

                if (isExplicitlyIncluded)
                {
                    return;
                }

                AssertRequiredPropertyIsNotIgnored(value, attributesPropertyInfo, jsonWriter.Path);
            }
        }

        private static void AssertRequiredPropertyIsNotIgnored(object value, PropertyInfo attribute, string path)
        {
            JsonPropertyAttribute jsonPropertyForAttribute = attribute.GetCustomAttributes<JsonPropertyAttribute>().Single();

            if (jsonPropertyForAttribute.Required is not (Required.Always or Required.AllowNull))
            {
                return;
            }

            bool isPropertyIgnored = DefaultValueEqualsCurrentValue(attribute, value);

            if (isPropertyIgnored)
            {
                throw new InvalidOperationException($"The following property should not be omitted: {path}.{jsonPropertyForAttribute.PropertyName}.");
            }
        }

        private static bool DefaultValueEqualsCurrentValue(PropertyInfo propertyInfo, object instance)
        {
            object? currentValue = propertyInfo.GetValue(instance);
            object? defaultValue = GetDefaultValue(propertyInfo.PropertyType);

            if (defaultValue == null)
            {
                return currentValue == null;
            }

            return defaultValue.Equals(currentValue);
        }

        private static object? GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }

    private sealed class SerializationScope
    {
        private bool _isFirstAttemptToConvertAttributes = true;
        public AttributeNamesContainer? AttributesObjectInScope { get; set; }

        public bool ShouldConvertAsAttributesObject(Type type)
        {
            if (!_isFirstAttemptToConvertAttributes || AttributesObjectInScope == null)
            {
                return false;
            }

            if (!AttributesObjectInScope.MatchesAttributesObjectType(type))
            {
                return false;
            }

            _isFirstAttemptToConvertAttributes = false;
            return true;
        }
    }

    private sealed class AttributeNamesContainer
    {
        private readonly ISet<string> _attributeNames;
        private readonly Type _attributesObjectType;

        public AttributeNamesContainer(ISet<string> attributeNames, Type attributesObjectType)
        {
            ArgumentGuard.NotNull(attributeNames);
            ArgumentGuard.NotNull(attributesObjectType);

            _attributeNames = attributeNames;
            _attributesObjectType = attributesObjectType;
        }

        public bool ContainsAttribute(string name)
        {
            return _attributeNames.Contains(name);
        }

        public bool MatchesAttributesObjectType(Type type)
        {
            return _attributesObjectType == type;
        }
    }

    private sealed class RequestDocumentRegistrationScope : IDisposable
    {
        private readonly JsonApiJsonConverter _jsonApiJsonConverter;
        private readonly object _requestDocument;

        public RequestDocumentRegistrationScope(JsonApiJsonConverter jsonApiJsonConverter, object requestDocument)
        {
            ArgumentGuard.NotNull(jsonApiJsonConverter);
            ArgumentGuard.NotNull(requestDocument);

            _jsonApiJsonConverter = jsonApiJsonConverter;
            _requestDocument = requestDocument;
        }

        public void Dispose()
        {
            _jsonApiJsonConverter.RemoveRegistration(_requestDocument);
        }
    }

    private sealed class JsonApiAttributeContractResolver : DefaultContractResolver
    {
        private readonly AttributeNamesContainer _alwaysIncludedAttributes;

        public JsonApiAttributeContractResolver(AttributeNamesContainer alwaysIncludedAttributes)
        {
            ArgumentGuard.NotNull(alwaysIncludedAttributes);

            _alwaysIncludedAttributes = alwaysIncludedAttributes;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            bool canOmitAttribute = property.Required != Required.Always;

            if (canOmitAttribute && _alwaysIncludedAttributes.MatchesAttributesObjectType(property.DeclaringType!))
            {
                if (_alwaysIncludedAttributes.ContainsAttribute(property.UnderlyingName!))
                {
                    property.NullValueHandling = NullValueHandling.Include;
                    property.DefaultValueHandling = DefaultValueHandling.Include;
                }
                else
                {
                    property.NullValueHandling = NullValueHandling.Ignore;
                    property.DefaultValueHandling = DefaultValueHandling.Ignore;
                }
            }

            return property;
        }
    }
}
