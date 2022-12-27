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
    public IDisposable OmitDefaultValuesForAttributesInRequestDocument<TRequestDocument, TAttributesObject>(TRequestDocument requestDocument,
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
                throw new ArgumentException($"The expression '{selector}' should select a single property. For example: 'article => article.Title'.");
            }
        }

        _jsonApiJsonConverter.RegisterRequestDocumentForAttributesOmission(requestDocument,
            new AttributesObjectInfo(attributeNames, typeof(TAttributesObject)));

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
        private readonly Dictionary<object, AttributesObjectInfo> _attributesObjectInfoByRequestDocument = new();
        private readonly Dictionary<Type, ISet<object>> _requestDocumentsByType = new();
        private SerializationScope? _serializationScope;

        public override bool CanRead => false;

        public void RegisterRequestDocumentForAttributesOmission(object requestDocument, AttributesObjectInfo attributesObjectInfo)
        {
            _attributesObjectInfoByRequestDocument[requestDocument] = attributesObjectInfo;

            Type requestDocumentType = requestDocument.GetType();

            if (!_requestDocumentsByType.ContainsKey(requestDocumentType))
            {
                _requestDocumentsByType[requestDocumentType] = new HashSet<object>();
            }

            _requestDocumentsByType[requestDocumentType].Add(requestDocument);
        }

        public void RemoveRegistration(object requestDocument)
        {
            if (_attributesObjectInfoByRequestDocument.ContainsKey(requestDocument))
            {
                _attributesObjectInfoByRequestDocument.Remove(requestDocument);

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
                AttributesObjectInfo? attributesObjectInfo = _serializationScope.AttributesObjectInScope;

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

            if (_attributesObjectInfoByRequestDocument.TryGetValue(value, out AttributesObjectInfo? attributesObjectInfo))
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

        private static void AssertObjectMatchesSerializationScope([SysNotNull] AttributesObjectInfo? attributesObjectInfo, object value)
        {
            Type objectType = value.GetType();

            if (attributesObjectInfo == null || !attributesObjectInfo.MatchesType(objectType))
            {
                throw new UnreachableCodeException();
            }
        }

        private static void SerializeAttributesObject(AttributesObjectInfo alwaysIncludedAttributes, JsonWriter writer, object value, JsonSerializer serializer)
        {
            AssertRequiredPropertiesAreNotExcluded(value, alwaysIncludedAttributes, writer);

            serializer.ContractResolver = new JsonApiAttributeContractResolver(alwaysIncludedAttributes);
            serializer.Serialize(writer, value);
        }

        private static void AssertRequiredPropertiesAreNotExcluded(object value, AttributesObjectInfo alwaysIncludedAttributes, JsonWriter jsonWriter)
        {
            PropertyInfo[] propertyInfos = value.GetType().GetProperties();

            foreach (PropertyInfo attributesPropertyInfo in propertyInfos)
            {
                bool isExplicitlyIncluded = alwaysIncludedAttributes.IsAttributeMarkedForInclusion(attributesPropertyInfo.Name);

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

            if (jsonPropertyForAttribute.Required != Required.Always)
            {
                return;
            }

            bool isPropertyIgnored = DefaultValueEqualsCurrentValue(attribute, value);

            if (isPropertyIgnored)
            {
                throw new JsonSerializationException(
                    $"Ignored property '{jsonPropertyForAttribute.PropertyName}' must have a value because it is required. Path '{path}'.");
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
        public AttributesObjectInfo? AttributesObjectInScope { get; set; }

        public bool ShouldConvertAsAttributesObject(Type type)
        {
            if (!_isFirstAttemptToConvertAttributes || AttributesObjectInScope == null)
            {
                return false;
            }

            if (!AttributesObjectInScope.MatchesType(type))
            {
                return false;
            }

            _isFirstAttemptToConvertAttributes = false;
            return true;
        }
    }

    private sealed class AttributesObjectInfo
    {
        private readonly ISet<string> _attributesMarkedForInclusion;
        private readonly Type _attributesObjectType;

        public AttributesObjectInfo(ISet<string> attributesMarkedForInclusion, Type attributesObjectType)
        {
            ArgumentGuard.NotNull(attributesMarkedForInclusion);
            ArgumentGuard.NotNull(attributesObjectType);

            _attributesMarkedForInclusion = attributesMarkedForInclusion;
            _attributesObjectType = attributesObjectType;
        }

        public bool IsAttributeMarkedForInclusion(string name)
        {
            return _attributesMarkedForInclusion.Contains(name);
        }

        public bool MatchesType(Type type)
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
        private readonly AttributesObjectInfo _attributesObjectInfo;

        public JsonApiAttributeContractResolver(AttributesObjectInfo attributesObjectInfo)
        {
            ArgumentGuard.NotNull(attributesObjectInfo);

            _attributesObjectInfo = attributesObjectInfo;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (_attributesObjectInfo.MatchesType(property.DeclaringType!))
            {
                if (_attributesObjectInfo.IsAttributeMarkedForInclusion(property.UnderlyingName!))
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
