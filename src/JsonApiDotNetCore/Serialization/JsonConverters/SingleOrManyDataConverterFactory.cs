using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.JsonConverters
{
    /// <summary>
    /// Converts <see cref="SingleOrManyData{T}" /> to/from JSON.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class SingleOrManyDataConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(SingleOrManyData<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type objectType = typeToConvert.GetGenericArguments()[0];
            Type converterType = typeof(SingleOrManyDataConverter<>).MakeGenericType(objectType);

            return (JsonConverter)Activator.CreateInstance(converterType, BindingFlags.Instance | BindingFlags.Public, null, null, null)!;
        }

        private sealed class SingleOrManyDataConverter<T> : JsonObjectConverter<SingleOrManyData<T>>
            where T : class, IResourceIdentity, new()
        {
            public override SingleOrManyData<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions serializerOptions)
            {
                var objects = new List<T?>();
                bool isManyData = false;
                bool hasCompletedToMany = false;

                do
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.EndArray:
                        {
                            hasCompletedToMany = true;
                            break;
                        }
                        case JsonTokenType.Null:
                        {
                            if (isManyData)
                            {
                                objects.Add(new T());
                            }

                            break;
                        }
                        case JsonTokenType.StartObject:
                        {
                            var resourceObject = ReadSubTree<T>(ref reader, serializerOptions);
                            objects.Add(resourceObject);
                            break;
                        }
                        case JsonTokenType.StartArray:
                        {
                            isManyData = true;
                            break;
                        }
                    }
                }
                while (isManyData && !hasCompletedToMany && reader.Read());

                object? data = isManyData ? objects : objects.FirstOrDefault();
                return new SingleOrManyData<T>(data);
            }

            public override void Write(Utf8JsonWriter writer, SingleOrManyData<T> value, JsonSerializerOptions options)
            {
                WriteSubTree(writer, value.Value, options);
            }
        }
    }
}
