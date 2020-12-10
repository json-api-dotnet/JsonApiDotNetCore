using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.Query
{
    [MarkdownExporter, SimpleJob(launchCount: 3, warmupCount: 10, targetCount: 20), MemoryDiagnoser]
    public class QueryParserBenchmarks
    {
        private readonly FakeRequestQueryStringAccessor _queryStringAccessor = new FakeRequestQueryStringAccessor();
        private readonly QueryStringReader _queryStringReaderForSort;
        private readonly QueryStringReader _queryStringReaderForAll;

        public QueryParserBenchmarks()
        {
            IJsonApiOptions options = new JsonApiOptions
            {
                EnableLegacyFilterNotation = true
            };

            IResourceGraph resourceGraph = DependencyFactory.CreateResourceGraph(options);

            var request = new JsonApiRequest
            {
                PrimaryResource = resourceGraph.GetResourceContext(typeof(BenchmarkResource)),
                IsCollection = true
            };

            _queryStringReaderForSort = CreateQueryParameterDiscoveryForSort(resourceGraph, request, options, _queryStringAccessor);
            _queryStringReaderForAll = CreateQueryParameterDiscoveryForAll(resourceGraph, request, options, _queryStringAccessor);
        }

        private static QueryStringReader CreateQueryParameterDiscoveryForSort(IResourceGraph resourceGraph,
            JsonApiRequest request, IJsonApiOptions options, FakeRequestQueryStringAccessor queryStringAccessor)
        {
            var sortReader = new SortQueryStringParameterReader(request, resourceGraph);
            
            var readers = new List<IQueryStringParameterReader>
            {
                sortReader
            };

            return new QueryStringReader(options, queryStringAccessor, readers, NullLoggerFactory.Instance);
        }

        private static QueryStringReader CreateQueryParameterDiscoveryForAll(IResourceGraph resourceGraph,
            JsonApiRequest request, IJsonApiOptions options, FakeRequestQueryStringAccessor queryStringAccessor)
        {
            var resourceFactory = new ResourceFactory(new ServiceContainer());

            var includeReader = new IncludeQueryStringParameterReader(request, resourceGraph, options);
            var filterReader = new FilterQueryStringParameterReader(request, resourceGraph, resourceFactory, options);
            var sortReader = new SortQueryStringParameterReader(request, resourceGraph);
            var sparseFieldSetReader = new SparseFieldSetQueryStringParameterReader(request, resourceGraph);
            var paginationReader = new PaginationQueryStringParameterReader(request, resourceGraph, options);
            var defaultsReader = new DefaultsQueryStringParameterReader(options);
            var nullsReader = new NullsQueryStringParameterReader(options);

            var readers = new List<IQueryStringParameterReader>
            {
                includeReader, filterReader, sortReader, sparseFieldSetReader, paginationReader, defaultsReader, nullsReader
            };

            return new QueryStringReader(options, queryStringAccessor, readers, NullLoggerFactory.Instance);
        }

        [Benchmark]
        public void AscendingSort()
        {
            var queryString = $"?sort={BenchmarkResourcePublicNames.NameAttr}";

            _queryStringAccessor.SetQueryString(queryString);
            _queryStringReaderForSort.ReadAll(null);
        }

        [Benchmark]
        public void DescendingSort()
        {
            var queryString = $"?sort=-{BenchmarkResourcePublicNames.NameAttr}";

            _queryStringAccessor.SetQueryString(queryString);
            _queryStringReaderForSort.ReadAll(null);
        }

        [Benchmark]
        public void ComplexQuery() => Run(100, () =>
        {
            var queryString = $"?filter[{BenchmarkResourcePublicNames.NameAttr}]=abc,eq:abc&sort=-{BenchmarkResourcePublicNames.NameAttr}&include=child&page[size]=1&fields[{BenchmarkResourcePublicNames.Type}]={BenchmarkResourcePublicNames.NameAttr}";

            _queryStringAccessor.SetQueryString(queryString);
            _queryStringReaderForAll.ReadAll(null);
        });

        private void Run(int iterations, Action action) { 
            for (int i = 0; i < iterations; i++)
                action();
        }

        private sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
        {
            public IQueryCollection Query { get; private set; }

            public void SetQueryString(string queryString)
            {
                Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
            }
        }
    }
}
