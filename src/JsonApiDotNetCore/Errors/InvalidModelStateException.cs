using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when ASP.NET ModelState validation fails.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidModelStateException : JsonApiException
    {
        public InvalidModelStateException(IReadOnlyDictionary<string, ModelStateEntry?> modelState, Type modelType, bool includeExceptionStackTraceInErrors,
            IResourceGraph resourceGraph, Func<Type, int, Type?>? getCollectionElementTypeCallback = null)
            : base(FromModelStateDictionary(modelState, modelType, resourceGraph, includeExceptionStackTraceInErrors, getCollectionElementTypeCallback))
        {
        }

        private static IEnumerable<ErrorObject> FromModelStateDictionary(IReadOnlyDictionary<string, ModelStateEntry?> modelState, Type modelType,
            IResourceGraph resourceGraph, bool includeExceptionStackTraceInErrors, Func<Type, int, Type?>? getCollectionElementTypeCallback)
        {
            ArgumentGuard.NotNull(modelState, nameof(modelState));
            ArgumentGuard.NotNull(modelType, nameof(modelType));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            List<ErrorObject> errorObjects = new();

            foreach ((ModelStateEntry entry, string? sourcePointer) in ResolveSourcePointers(modelState, modelType, resourceGraph,
                getCollectionElementTypeCallback))
            {
                AppendToErrorObjects(entry, errorObjects, sourcePointer, includeExceptionStackTraceInErrors);
            }

            return errorObjects;
        }

        private static IEnumerable<(ModelStateEntry entry, string? sourcePointer)> ResolveSourcePointers(
            IReadOnlyDictionary<string, ModelStateEntry?> modelState, Type modelType, IResourceGraph resourceGraph,
            Func<Type, int, Type?>? getCollectionElementTypeCallback)
        {
            foreach (string key in modelState.Keys)
            {
                var rootSegment = ModelStateKeySegment.Create(modelType, key, getCollectionElementTypeCallback);
                string? sourcePointer = ResolveSourcePointer(rootSegment, resourceGraph);

                yield return (modelState[key]!, sourcePointer);
            }
        }

        private static string? ResolveSourcePointer(ModelStateKeySegment segment, IResourceGraph resourceGraph)
        {
            if (segment is ArrayIndexerSegment indexerSegment)
            {
                return ResolveSourcePointerInArrayIndexer(indexerSegment, resourceGraph);
            }

            if (segment is PropertySegment propertySegment)
            {
                if (segment.IsInComplexType)
                {
                    return ResolveSourcePointerInComplexType(propertySegment, resourceGraph);
                }

                if (propertySegment.PropertyName == nameof(OperationContainer.Resource) && propertySegment.Parent != null &&
                    propertySegment.Parent.ModelType == typeof(IList<OperationContainer>))
                {
                    // Special case: Stepping over OperationContainer.Resource property.

                    if (segment.GetNextSegment(propertySegment.ModelType, false, $"{segment.SourcePointer}/data") is not PropertySegment nextPropertySegment)
                    {
                        return null;
                    }

                    propertySegment = nextPropertySegment;
                }

                return ResolveSourcePointerInResourceField(propertySegment, resourceGraph);
            }

            return segment.SourcePointer;
        }

        private static string? ResolveSourcePointerInArrayIndexer(ArrayIndexerSegment segment, IResourceGraph resourceGraph)
        {
            string sourcePointer = $"{segment.SourcePointer ?? "/atomic:operations"}[{segment.ArrayIndex}]";
            Type elementType = segment.GetCollectionElementType();

            ModelStateKeySegment? nextSegment = segment.GetNextSegment(elementType, segment.IsInComplexType, sourcePointer);
            return nextSegment != null ? ResolveSourcePointer(nextSegment, resourceGraph) : sourcePointer;
        }

        private static string? ResolveSourcePointerInComplexType(PropertySegment segment, IResourceGraph resourceGraph)
        {
            PropertyInfo? property = segment.ModelType.GetProperty(segment.PropertyName);

            if (property == null)
            {
                return null;
            }

            string publicName = PropertySegment.GetPublicNameForProperty(property);
            string? sourcePointer = segment.SourcePointer != null ? $"{segment.SourcePointer}/{publicName}" : null;

            ModelStateKeySegment? nextSegment = segment.GetNextSegment(property.PropertyType, true, sourcePointer);
            return nextSegment != null ? ResolveSourcePointer(nextSegment, resourceGraph) : sourcePointer;
        }

        private static string? ResolveSourcePointerInResourceField(PropertySegment segment, IResourceGraph resourceGraph)
        {
            ResourceType? resourceType = resourceGraph.FindResourceType(segment.ModelType);

            if (resourceType != null)
            {
                AttrAttribute? attribute = resourceType.FindAttributeByPropertyName(segment.PropertyName);

                if (attribute != null)
                {
                    return ResolveSourcePointerInAttribute(segment, attribute, resourceGraph);
                }

                RelationshipAttribute? relationship = resourceType.FindRelationshipByPropertyName(segment.PropertyName);

                if (relationship != null)
                {
                    return ResolveSourcePointerInRelationship(segment, relationship, resourceGraph);
                }
            }

            return null;
        }

        private static string? ResolveSourcePointerInAttribute(PropertySegment segment, AttrAttribute attribute, IResourceGraph resourceGraph)
        {
            string sourcePointer = attribute.Property.Name == nameof(Identifiable<object>.Id)
                ? $"{segment.SourcePointer ?? "/data"}/{attribute.PublicName}"
                : $"{segment.SourcePointer ?? "/data"}/attributes/{attribute.PublicName}";

            ModelStateKeySegment? nextSegment = segment.GetNextSegment(attribute.Property.PropertyType, true, sourcePointer);
            return nextSegment != null ? ResolveSourcePointer(nextSegment, resourceGraph) : sourcePointer;
        }

        private static string? ResolveSourcePointerInRelationship(PropertySegment segment, RelationshipAttribute relationship, IResourceGraph resourceGraph)
        {
            string sourcePointer = $"{segment.SourcePointer ?? "/data"}/relationships/{relationship.PublicName}/data";

            ModelStateKeySegment? nextSegment = segment.GetNextSegment(relationship.RightType.ClrType, false, sourcePointer);
            return nextSegment != null ? ResolveSourcePointer(nextSegment, resourceGraph) : sourcePointer;
        }

        private static void AppendToErrorObjects(ModelStateEntry entry, List<ErrorObject> errorObjects, string? sourcePointer,
            bool includeExceptionStackTraceInErrors)
        {
            foreach (ModelError error in entry.Errors)
            {
                if (error.Exception is JsonApiException jsonApiException)
                {
                    errorObjects.AddRange(jsonApiException.Errors);
                }
                else
                {
                    ErrorObject errorObject = FromModelError(error, sourcePointer, includeExceptionStackTraceInErrors);
                    errorObjects.Add(errorObject);
                }
            }
        }

        private static ErrorObject FromModelError(ModelError modelError, string? sourcePointer, bool includeExceptionStackTraceInErrors)
        {
            var error = new ErrorObject(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Input validation failed.",
                Detail = modelError.Exception is TooManyModelErrorsException tooManyException ? tooManyException.Message : modelError.ErrorMessage,
                Source = sourcePointer == null
                    ? null
                    : new ErrorSource
                    {
                        Pointer = sourcePointer
                    }
            };

            if (includeExceptionStackTraceInErrors && modelError.Exception != null)
            {
                Exception exception = modelError.Exception.Demystify();
                string[] stackTraceLines = exception.ToString().Split(Environment.NewLine);

                if (stackTraceLines.Any())
                {
                    error.Meta ??= new Dictionary<string, object?>();
                    error.Meta["StackTrace"] = stackTraceLines;
                }
            }

            return error;
        }

        /// <summary>
        /// Base type that represents a segment in a ModelState key.
        /// </summary>
        private abstract class ModelStateKeySegment
        {
            private const char Dot = '.';
            private const char BracketOpen = '[';
            private const char BracketClose = ']';
            private static readonly char[] KeySegmentStartTokens = ArrayFactory.Create(Dot, BracketOpen);

            // The right part of the full key, which nested segments are produced from.
            private readonly string _nextKey;

            // Enables to resolve the runtime-type of a collection element, such as the resource type in an atomic:operation.
            protected Func<Type, int, Type?>? GetCollectionElementTypeCallback { get; }

            // In case of a property, its declaring type. In case of an indexer, the collection type or collection element type (in case the parent is a relationship).
            public Type ModelType { get; }

            // Indicates we're in a complex object, so to determine public name, inspect [JsonPropertyName] instead of [Attr], [HasOne] etc.
            public bool IsInComplexType { get; }

            // The source pointer we've built up, so far. This is null whenever input is not recognized.
            public string? SourcePointer { get; }

            public ModelStateKeySegment? Parent { get; }

            protected ModelStateKeySegment(Type modelType, bool isInComplexType, string nextKey, string? sourcePointer, ModelStateKeySegment? parent,
                Func<Type, int, Type?>? getCollectionElementTypeCallback)
            {
                ArgumentGuard.NotNull(modelType, nameof(modelType));
                ArgumentGuard.NotNull(nextKey, nameof(nextKey));

                ModelType = modelType;
                IsInComplexType = isInComplexType;
                _nextKey = nextKey;
                SourcePointer = sourcePointer;
                Parent = parent;
                GetCollectionElementTypeCallback = getCollectionElementTypeCallback;
            }

            public ModelStateKeySegment? GetNextSegment(Type modelType, bool isInComplexType, string? sourcePointer)
            {
                ArgumentGuard.NotNull(modelType, nameof(modelType));

                return _nextKey == string.Empty
                    ? null
                    : CreateSegment(modelType, _nextKey, isInComplexType, this, sourcePointer, GetCollectionElementTypeCallback);
            }

            public static ModelStateKeySegment Create(Type modelType, string key, Func<Type, int, Type?>? getCollectionElementTypeCallback)
            {
                ArgumentGuard.NotNull(modelType, nameof(modelType));
                ArgumentGuard.NotNull(key, nameof(key));

                return CreateSegment(modelType, key, false, null, null, getCollectionElementTypeCallback);
            }

            private static ModelStateKeySegment CreateSegment(Type modelType, string key, bool isInComplexType, ModelStateKeySegment? parent,
                string? sourcePointer, Func<Type, int, Type?>? getCollectionElementTypeCallback)
            {
                string? segmentValue = null;
                string? nextKey = null;

                int segmentEndIndex = key.IndexOfAny(KeySegmentStartTokens);

                if (segmentEndIndex == 0 && key[0] == BracketOpen)
                {
                    int bracketCloseIndex = key.IndexOf(BracketClose);

                    if (bracketCloseIndex != -1)
                    {
                        segmentValue = key[1.. bracketCloseIndex];

                        int nextKeyStartIndex = key.Length > bracketCloseIndex + 1 && key[bracketCloseIndex + 1] == Dot
                            ? bracketCloseIndex + 2
                            : bracketCloseIndex + 1;

                        nextKey = key[nextKeyStartIndex..];

                        if (int.TryParse(segmentValue, out int indexValue))
                        {
                            return new ArrayIndexerSegment(indexValue, modelType, isInComplexType, nextKey, sourcePointer, parent,
                                getCollectionElementTypeCallback);
                        }

                        // If the value between brackets is not numeric, consider it an unspeakable property. For example:
                        // "Foo[Bar]" instead of "Foo.Bar". Its unclear when this happens, but ASP.NET source contains tests for such keys.
                    }
                }

                if (segmentValue == null)
                {
                    segmentValue = segmentEndIndex == -1 ? key : key[..segmentEndIndex];

                    nextKey = segmentEndIndex != -1 && key.Length > segmentEndIndex && key[segmentEndIndex] == Dot
                        ? key[(segmentEndIndex + 1)..]
                        : key[segmentValue.Length..];
                }

                // Workaround for a quirk in ModelState validation. Some controller action methods have an 'id' parameter before the [FromBody] parameter.
                // When a validation error occurs on top-level 'Id' in the request body, its key contains 'id' instead of 'Id' (the error message is correct, though).
                // We compensate for that case here, so that we'll find 'Id' in the resource graph when building the source pointer.
                if (segmentValue == "id")
                {
                    segmentValue = "Id";
                }

                return new PropertySegment(segmentValue, modelType, isInComplexType, nextKey!, sourcePointer, parent, getCollectionElementTypeCallback);
            }
        }

        /// <summary>
        /// Represents an array indexer in a ModelState key, such as "1" in "Customer.Orders[1].Amount".
        /// </summary>
        private sealed class ArrayIndexerSegment : ModelStateKeySegment
        {
            private static readonly CollectionConverter CollectionConverter = new();

            public int ArrayIndex { get; }

            public ArrayIndexerSegment(int arrayIndex, Type modelType, bool isInComplexType, string nextKey, string? sourcePointer,
                ModelStateKeySegment? parent, Func<Type, int, Type?>? getCollectionElementTypeCallback)
                : base(modelType, isInComplexType, nextKey, sourcePointer, parent, getCollectionElementTypeCallback)
            {
                ArrayIndex = arrayIndex;
            }

            public Type GetCollectionElementType()
            {
                Type? type = GetCollectionElementTypeCallback?.Invoke(ModelType, ArrayIndex);
                return type ?? GetDeclaredCollectionElementType();
            }

            private Type GetDeclaredCollectionElementType()
            {
                if (ModelType != typeof(string))
                {
                    Type? elementType = CollectionConverter.FindCollectionElementType(ModelType);

                    if (elementType != null)
                    {
                        return elementType;
                    }
                }

                // In case of a to-many relationship, the ModelType already contains the element type.
                return ModelType;
            }
        }

        /// <summary>
        /// Represents a property in a ModelState key, such as "Orders" in "Customer.Orders[1].Amount".
        /// </summary>
        private sealed class PropertySegment : ModelStateKeySegment
        {
            public string PropertyName { get; }

            public PropertySegment(string propertyName, Type modelType, bool isInComplexType, string nextKey, string? sourcePointer,
                ModelStateKeySegment? parent, Func<Type, int, Type?>? getCollectionElementTypeCallback)
                : base(modelType, isInComplexType, nextKey, sourcePointer, parent, getCollectionElementTypeCallback)
            {
                ArgumentGuard.NotNull(propertyName, nameof(propertyName));

                PropertyName = propertyName;
            }

            public static string GetPublicNameForProperty(PropertyInfo property)
            {
                ArgumentGuard.NotNull(property, nameof(property));

                var jsonNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>(true);
                return jsonNameAttribute?.Name ?? property.Name;
            }
        }
    }
}
