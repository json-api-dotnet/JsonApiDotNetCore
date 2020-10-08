using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Benchmarks.Serialization
{
    [MarkdownExporter]
    public class JsonApiDeserializerBenchmarks
    {
        private static readonly string Content = JsonConvert.SerializeObject(new Document
        {
            Data = new ResourceObject
            {
                Type = BenchmarkResourcePublicNames.Type,
                Id = "1",
                Attributes = new Dictionary<string, object>
                {
                    {
                        "name",
                        Guid.NewGuid().ToString()
                    }
                }
            }
        });

        private readonly IJsonApiDeserializer _jsonApiDeserializer;

        public JsonApiDeserializerBenchmarks()
        {
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = DependencyFactory.CreateResourceGraph(options);
            var targetedFields = new TargetedFields();
            var request = new JsonApiRequest();
            _jsonApiDeserializer = new RequestDeserializer(resourceGraph, new ResourceFactory(new ServiceContainer()), targetedFields, new HttpContextAccessor(), request);
        }

        [Benchmark]
        public object DeserializeSimpleObject() => _jsonApiDeserializer.Deserialize(Content);
    }
}
