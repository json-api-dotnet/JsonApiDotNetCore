using System;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using Moq;

namespace Benchmarks.Serialization
{
    // ReSharper disable once ClassCanBeSealed.Global
    [MarkdownExporter]
    public class JsonApiSerializerBenchmarks
    {
        private static readonly BenchmarkResource Content = new()
        {
            Id = 123,
            Name = Guid.NewGuid().ToString()
        };

        private readonly DependencyFactory _dependencyFactory = new();
        private readonly IJsonApiSerializer _jsonApiSerializer;

        public JsonApiSerializerBenchmarks()
        {
            var options = new JsonApiOptions();
            var request = new JsonApiRequest();

            IResourceGraph resourceGraph = _dependencyFactory.CreateResourceGraph(options);

            var constraintProviders = new IQueryConstraintProvider[]
            {
                new SparseFieldSetQueryStringParameterReader(request, resourceGraph)
            };

            IResourceDefinitionAccessor resourceDefinitionAccessor = new Mock<IResourceDefinitionAccessor>().Object;
            var sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);

            IMetaBuilder metaBuilder = new Mock<IMetaBuilder>().Object;
            ILinkBuilder linkBuilder = new Mock<ILinkBuilder>().Object;
            IIncludedResourceObjectBuilder includeBuilder = new Mock<IIncludedResourceObjectBuilder>().Object;

            IFieldsToSerialize fieldsToSerialize =
                new FieldsToSerialize(resourceGraph, constraintProviders, resourceDefinitionAccessor, request, sparseFieldSetCache);

            var resourceObjectBuilder = new ResourceObjectBuilder(resourceGraph, options);

            _jsonApiSerializer = new ResponseSerializer<BenchmarkResource>(metaBuilder, linkBuilder, includeBuilder, fieldsToSerialize, resourceObjectBuilder,
                resourceDefinitionAccessor, sparseFieldSetCache, options);
        }

        [Benchmark]
        public object SerializeSimpleObject()
        {
            return _jsonApiSerializer.Serialize(Content);
        }
    }
}
