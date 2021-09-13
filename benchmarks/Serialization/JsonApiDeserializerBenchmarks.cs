using System.ComponentModel.Design;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Http;

namespace Benchmarks.Serialization
{
    // ReSharper disable once ClassCanBeSealed.Global
    [MarkdownExporter]
    public class JsonApiDeserializerBenchmarks
    {
        private static readonly string RequestBody = JsonSerializer.Serialize(new
        {
            data = new
            {
                type = BenchmarkResourcePublicNames.Type,
                id = "1",
                attributes = new
                {
                }
            }
        });

        private readonly DependencyFactory _dependencyFactory = new();
        private readonly IJsonApiDeserializer _jsonApiDeserializer;

        public JsonApiDeserializerBenchmarks()
        {
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = _dependencyFactory.CreateResourceGraph(options);

            var serviceContainer = new ServiceContainer();
            var resourceDefinitionAccessor = new ResourceDefinitionAccessor(resourceGraph, serviceContainer);

            serviceContainer.AddService(typeof(IResourceDefinitionAccessor), resourceDefinitionAccessor);
            serviceContainer.AddService(typeof(IResourceDefinition<BenchmarkResource>), new JsonApiResourceDefinition<BenchmarkResource>(resourceGraph));

            var targetedFields = new TargetedFields();
            var request = new JsonApiRequest();
            var resourceFactory = new ResourceFactory(serviceContainer);
            var httpContextAccessor = new HttpContextAccessor();

            _jsonApiDeserializer = new RequestDeserializer(resourceGraph, resourceFactory, targetedFields, httpContextAccessor, request, options,
                resourceDefinitionAccessor);
        }

        [Benchmark]
        public object DeserializeSimpleObject()
        {
            return _jsonApiDeserializer.Deserialize(RequestBody);
        }
    }
}
