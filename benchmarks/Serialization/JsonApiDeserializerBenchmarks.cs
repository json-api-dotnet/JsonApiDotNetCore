using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
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
            _jsonApiDeserializer = new RequestDeserializer(resourceGraph, new DefaultResourceFactory(new ServiceContainer()), targetedFields, new HttpContextAccessor());
        }

        [Benchmark]
        public object DeserializeSimpleObject() => _jsonApiDeserializer.Deserialize(Content);
    }
}
