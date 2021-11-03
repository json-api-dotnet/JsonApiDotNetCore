using System;
using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.Internal;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.QueryString
{
    // ReSharper disable once ClassCanBeSealed.Global
    [MarkdownExporter]
    [SimpleJob(3, 10, 20)]
    [MemoryDiagnoser]
    public class QueryStringParserBenchmarks
    {
        private readonly FakeRequestQueryStringAccessor _queryStringAccessor = new();
        private readonly QueryStringReader _queryStringReader;

        public QueryStringParserBenchmarks()
        {
            IJsonApiOptions options = new JsonApiOptions
            {
                EnableLegacyFilterNotation = true
            };

            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<QueryableResource>("alt-resource-name").Build();

            var request = new JsonApiRequest
            {
                PrimaryResourceType = resourceGraph.GetResourceType(typeof(QueryableResource)),
                IsCollection = true
            };

            var resourceFactory = new ResourceFactory(new ServiceContainer());

            var includeReader = new IncludeQueryStringParameterReader(request, resourceGraph, options);
            var filterReader = new FilterQueryStringParameterReader(request, resourceGraph, resourceFactory, options);
            var sortReader = new SortQueryStringParameterReader(request, resourceGraph);
            var sparseFieldSetReader = new SparseFieldSetQueryStringParameterReader(request, resourceGraph);
            var paginationReader = new PaginationQueryStringParameterReader(request, resourceGraph, options);

            IQueryStringParameterReader[] readers = ArrayFactory.Create<IQueryStringParameterReader>(includeReader, filterReader, sortReader,
                sparseFieldSetReader, paginationReader);

            _queryStringReader = new QueryStringReader(options, _queryStringAccessor, readers, NullLoggerFactory.Instance);
        }

        [Benchmark]
        public void AscendingSort()
        {
            const string queryString = "?sort=alt-attr-name";

            _queryStringAccessor.SetQueryString(queryString);
            _queryStringReader.ReadAll(null);
        }

        [Benchmark]
        public void DescendingSort()
        {
            const string queryString = "?sort=-alt-attr-name";

            _queryStringAccessor.SetQueryString(queryString);
            _queryStringReader.ReadAll(null);
        }

        [Benchmark]
        public void ComplexQuery()
        {
            Run(100, () =>
            {
                const string queryString =
                    "?filter[alt-attr-name]=abc,eq:abc&sort=-alt-attr-name&include=child&page[size]=1&fields[alt-resource-name]=alt-attr-name";

                _queryStringAccessor.SetQueryString(queryString);
                _queryStringReader.ReadAll(null);
            });
        }

        private void Run(int iterations, Action action)
        {
            for (int index = 0; index < iterations; index++)
            {
                action();
            }
        }

        private sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
        {
            public IQueryCollection Query { get; private set; } = new QueryCollection();

            public void SetQueryString(string queryString)
            {
                Query = new QueryCollection(QueryHelpers.ParseQuery(queryString));
            }
        }
    }
}
