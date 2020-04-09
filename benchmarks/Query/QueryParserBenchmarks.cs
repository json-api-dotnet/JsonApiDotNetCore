using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.QueryParameterServices.Common;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.Query
{
    [MarkdownExporter, SimpleJob(launchCount: 3, warmupCount: 10, targetCount: 20), MemoryDiagnoser]
    public class QueryParserBenchmarks
    {
        private readonly FakeRequestQueryStringAccessor _queryStringAccessor = new FakeRequestQueryStringAccessor();
        private readonly QueryParameterParser _queryParameterParserForSort;
        private readonly QueryParameterParser _queryParameterParserForAll;

        public QueryParserBenchmarks()
        {
            IJsonApiOptions options = new JsonApiOptions();
            IResourceGraph resourceGraph = DependencyFactory.CreateResourceGraph(options);
            
            var currentRequest = new CurrentRequest();
            currentRequest.SetRequestResource(resourceGraph.GetResourceContext(typeof(BenchmarkResource)));

            IResourceDefinitionProvider resourceDefinitionProvider = DependencyFactory.CreateResourceDefinitionProvider(resourceGraph);

            _queryParameterParserForSort = CreateQueryParameterDiscoveryForSort(resourceGraph, currentRequest, resourceDefinitionProvider, options, _queryStringAccessor);
            _queryParameterParserForAll = CreateQueryParameterDiscoveryForAll(resourceGraph, currentRequest, resourceDefinitionProvider, options, _queryStringAccessor);
        }

        private static QueryParameterParser CreateQueryParameterDiscoveryForSort(IResourceGraph resourceGraph,
            CurrentRequest currentRequest, IResourceDefinitionProvider resourceDefinitionProvider,
            IJsonApiOptions options, FakeRequestQueryStringAccessor queryStringAccessor)
        {
            ISortService sortService = new SortService(resourceDefinitionProvider, resourceGraph, currentRequest);

            var queryServices = new List<IQueryParameterService>
            {
                sortService
            };

            return new QueryParameterParser(options, queryStringAccessor, queryServices, NullLoggerFactory.Instance);
        }

        private static QueryParameterParser CreateQueryParameterDiscoveryForAll(IResourceGraph resourceGraph,
            CurrentRequest currentRequest, IResourceDefinitionProvider resourceDefinitionProvider,
            IJsonApiOptions options, FakeRequestQueryStringAccessor queryStringAccessor)
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

            return new QueryParameterParser(options, queryStringAccessor, queryServices, NullLoggerFactory.Instance);
        }

        [Benchmark]
        public void AscendingSort()
        {
            var queryString = $"?sort={BenchmarkResourcePublicNames.NameAttr}";

            _queryStringAccessor.SetQueryString(queryString);
            _queryParameterParserForSort.Parse(null);
        }

        [Benchmark]
        public void DescendingSort()
        {
            var queryString = $"?sort=-{BenchmarkResourcePublicNames.NameAttr}";

            _queryStringAccessor.SetQueryString(queryString);
            _queryParameterParserForSort.Parse(null);
        }

        [Benchmark]
        public void ComplexQuery() => Run(100, () =>
        {
            var queryString = $"?filter[{BenchmarkResourcePublicNames.NameAttr}]=abc,eq:abc&sort=-{BenchmarkResourcePublicNames.NameAttr}&include=child&page[size]=1&fields={BenchmarkResourcePublicNames.NameAttr}";

            _queryStringAccessor.SetQueryString(queryString);
            _queryParameterParserForAll.Parse(null);
        });

        private void Run(int iterations, Action action) { 
            for (int i = 0; i < iterations; i++)
                action();
        }

        private sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
        {
            public QueryString QueryString { get; private set; }
            public IQueryCollection Query { get; private set; }

            public void SetQueryString(string queryString)
            {
                QueryString = new QueryString(queryString);
                Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
            }
        }
    }
}
