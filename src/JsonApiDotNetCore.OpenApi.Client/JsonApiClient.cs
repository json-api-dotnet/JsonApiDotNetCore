using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        ArgumentGuard.NotNull(settings, nameof(settings));

        settings.Converters.Add(_jsonApiJsonConverter);
    }

    /// <inheritdoc />
    public IDisposable RegisterAttributesForRequestDocument<TRequestDocument, TAttributesObject>(TRequestDocument requestDocument,
        params Expression<Func<TAttributesObject, object?>>[] alwaysIncludedAttributeSelectors)
        where TRequestDocument : class
    {
        ArgumentGuard.NotNull(requestDocument, nameof(requestDocument));

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

        _jsonApiJsonConverter.RegisterRequestDocument(requestDocument, new AttributeNamesContainer(attributeNames, typeof(TAttributesObject)));

        return new AttributesRegistrationScope(_jsonApiJsonConverter, requestDocument);
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
        private readonly Dictionary<object, AttributeNamesContainer> _alwaysIncludedAttributesPerRequestDocumentInstance = new();
        private readonly Dictionary<Type, ISet<object>> _requestDocumentInstancesPerRequestDocumentType = new();
        private bool _isSerializing;

        public override bool CanRead => false;

        public void RegisterRequestDocument(object requestDocument, AttributeNamesContainer attributes)
        {
            _alwaysIncludedAttributesPerRequestDocumentInstance[requestDocument] = attributes;

            Type requestDocumentType = requestDocument.GetType();

            if (!_requestDocumentInstancesPerRequestDocumentType.ContainsKey(requestDocumentType))
            {
                _requestDocumentInstancesPerRequestDocumentType[requestDocumentType] = new HashSet<object>();
            }

            _requestDocumentInstancesPerRequestDocumentType[requestDocumentType].Add(requestDocument);
        }

        public void RemoveAttributeRegistration(object requestDocument)
        {
            if (_alwaysIncludedAttributesPerRequestDocumentInstance.ContainsKey(requestDocument))
            {
                _alwaysIncludedAttributesPerRequestDocumentInstance.Remove(requestDocument);

                Type requestDocumentType = requestDocument.GetType();
                _requestDocumentInstancesPerRequestDocumentType[requestDocumentType].Remove(requestDocument);

                if (!_requestDocumentInstancesPerRequestDocumentType[requestDocumentType].Any())
                {
                    _requestDocumentInstancesPerRequestDocumentType.Remove(requestDocumentType);
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            return !_isSerializing && _requestDocumentInstancesPerRequestDocumentType.ContainsKey(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new Exception("This code should not be reachable.");
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ArgumentGuard.NotNull(writer, nameof(writer));
            ArgumentGuard.NotNull(serializer, nameof(serializer));

            if (value != null)
            {
                if (_alwaysIncludedAttributesPerRequestDocumentInstance.ContainsKey(value))
                {
                    AttributeNamesContainer attributeNamesContainer = _alwaysIncludedAttributesPerRequestDocumentInstance[value];
                    serializer.ContractResolver = new JsonApiDocumentContractResolver(attributeNamesContainer);
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

    private sealed class AttributeNamesContainer
    {
        private readonly ISet<string> _attributeNames;
        private readonly Type _containerType;

        public AttributeNamesContainer(ISet<string> attributeNames, Type containerType)
        {
            ArgumentGuard.NotNull(attributeNames, nameof(attributeNames));
            ArgumentGuard.NotNull(containerType, nameof(containerType));

            _attributeNames = attributeNames;
            _containerType = containerType;
        }

        public bool ContainsAttribute(string name)
        {
            return _attributeNames.Contains(name);
        }

        public bool ContainerMatchesType(Type type)
        {
            return _containerType == type;
        }
    }

    private sealed class AttributesRegistrationScope : IDisposable
    {
        private readonly JsonApiJsonConverter _jsonApiJsonConverter;
        private readonly object _requestDocument;

        public AttributesRegistrationScope(JsonApiJsonConverter jsonApiJsonConverter, object requestDocument)
        {
            ArgumentGuard.NotNull(jsonApiJsonConverter, nameof(jsonApiJsonConverter));
            ArgumentGuard.NotNull(requestDocument, nameof(requestDocument));

            _jsonApiJsonConverter = jsonApiJsonConverter;
            _requestDocument = requestDocument;
        }

        public void Dispose()
        {
            _jsonApiJsonConverter.RemoveAttributeRegistration(_requestDocument);
        }
    }

    private sealed class JsonApiDocumentContractResolver : DefaultContractResolver
    {
        private readonly AttributeNamesContainer _attributeNamesContainer;

        public JsonApiDocumentContractResolver(AttributeNamesContainer attributeNamesContainer)
        {
            ArgumentGuard.NotNull(attributeNamesContainer, nameof(attributeNamesContainer));

            _attributeNamesContainer = attributeNamesContainer;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (_attributeNamesContainer.ContainerMatchesType(property.DeclaringType!))
            {
                if (_attributeNamesContainer.ContainsAttribute(property.UnderlyingName!))
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
