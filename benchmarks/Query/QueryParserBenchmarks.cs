using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Benchmarks.Query
{
    [MarkdownExporter, SimpleJob(launchCount: 3, warmupCount: 10, targetCount: 20), MemoryDiagnoser]
    public class QueryParserBenchmarks
    {
        private readonly QueryParameterDiscovery _queryParameterDiscoveryForSort;
        private readonly QueryParameterDiscovery _queryParameterDiscoveryForAll;

        public QueryParserBenchmarks()
        {
            IJsonApiOptions options = new JsonApiOptions();
            IResourceGraph resourceGraph = DependencyFactory.CreateResourceGraph();
            
            var currentRequest = new CurrentRequest();
            currentRequest.SetRequestResource(resourceGraph.GetResourceContext(typeof(BenchmarkResource)));

            IResourceDefinitionProvider resourceDefinitionProvider = DependencyFactory.CreateResourceDefinitionProvider(resourceGraph);

            _queryParameterDiscoveryForSort = CreateQueryParameterDiscoveryForSort(resourceGraph, currentRequest, resourceDefinitionProvider, options);
            _queryParameterDiscoveryForAll = CreateQueryParameterDiscoveryForAll(resourceGraph, currentRequest, resourceDefinitionProvider, options);
        }

        private static QueryParameterDiscovery CreateQueryParameterDiscoveryForSort(IResourceGraph resourceGraph,
            CurrentRequest currentRequest, IResourceDefinitionProvider resourceDefinitionProvider, IJsonApiOptions options)
        {
            ISortService sortService = new SortService(resourceDefinitionProvider, resourceGraph, currentRequest);

            var queryServices = new List<IQueryParameterService>
            {
                sortService
            };

            return new QueryParameterDiscovery(options, queryServices);
        }

        private static QueryParameterDiscovery CreateQueryParameterDiscoveryForAll(IResourceGraph resourceGraph,
            CurrentRequest currentRequest, IResourceDefinitionProvider resourceDefinitionProvider, IJsonApiOptions options)
        {
            IIncludeService includeService = new IncludeService(resourceGraph, currentRequest);
            IFilterService filterService = new FilterService(resourceDefinitionProvider, resourceGraph, currentRequest);
            ISortService sortService = new SortService(resourceDefinitionProvider, resourceGraph, currentRequest);
            ISparseFieldsService sparseFieldsService = new SparseFieldsService(resourceGraph, currentRequest);
            IPageService pageService = new PageService(options, resourceGraph, currentRequest);
            IOmitDefaultService omitDefaultService = new OmitDefaultService(options);
            IOmitNullService omitNullService = new OmitNullService(options);

            var queryServices = new List<IQueryParameterService>
            {
                includeService, filterService, sortService, sparseFieldsService, pageService, omitDefaultService,
                omitNullService
            };

            return new QueryParameterDiscovery(options, queryServices);
        }

        [Benchmark]
        public void AscendingSort() => _queryParameterDiscoveryForSort.Parse(new QueryCollection(
            new Dictionary<string, StringValues>
            {
                {"sort", BenchmarkResourcePublicNames.NameAttr}
            }
        ), null);

        [Benchmark]
        public void DescendingSort() => _queryParameterDiscoveryForSort.Parse(new QueryCollection(
            new Dictionary<string, StringValues>
            {
                {"sort", $"-{BenchmarkResourcePublicNames.NameAttr}"}
            }
        ), null);

        [Benchmark]
        public void ComplexQuery() => Run(100, () => _queryParameterDiscoveryForAll.Parse(new QueryCollection(
            new Dictionary<string, StringValues>
            {
                {$"filter[{BenchmarkResourcePublicNames.NameAttr}]", new StringValues(new[] {"abc", "eq:abc"})},
                {"sort", $"-{BenchmarkResourcePublicNames.NameAttr}"},
                {"include", "child"},
                {"page[size]", "1"},
                {"fields", BenchmarkResourcePublicNames.NameAttr}
            }
        ), null));

        private void Run(int iterations, Action action) { 
            for (int i = 0; i < iterations; i++)
                action();
        }
    }
}
