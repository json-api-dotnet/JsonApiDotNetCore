using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Benchmarks.Serialization {
    public class JsonApiSerializer_Benchmarks {
        private const string TYPE_NAME = "simple-types";
        private static readonly SimpleType Content = new SimpleType();

        private readonly JsonApiSerializer _jsonApiSerializer;

        public JsonApiSerializer_Benchmarks() {
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<SimpleType>(TYPE_NAME);
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var genericProcessorFactoryMock = new Mock<IGenericProcessorFactory>();

            var documentBuilder = new DocumentBuilder(jsonApiContextMock.Object);
            _jsonApiSerializer = new JsonApiSerializer(jsonApiContextMock.Object, documentBuilder);
        }

        [Benchmark]
        public object DeserializeSimpleObject() => _jsonApiSerializer.Serialize(Content);

        private class SimpleType : Identifiable {
            [Attr("name")]
            public string Name { get; set; }
        }
    }
}
