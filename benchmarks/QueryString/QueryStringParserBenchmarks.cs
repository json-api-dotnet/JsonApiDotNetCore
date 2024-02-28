using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using Benchmarks.Tools;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.QueryString;

// ReSharper disable once ClassCanBeSealed.Global
[MarkdownExporter]
[SimpleJob(3, 10, 20)]
[MemoryDiagnoser]
public class QueryStringParserBenchmarks
{
    private readonly FakeRequestQueryStringAccessor _queryStringAccessor = new();
    private QueryStringReader _queryStringReader = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        IJsonApiOptions options = new JsonApiOptions();

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<QueryableResource, int>("alt-resource-name").Build();

        var request = new JsonApiRequest
        {
            PrimaryResourceType = resourceGraph.GetResourceType(typeof(QueryableResource)),
            IsCollection = true
        };

        var resourceFactory = new ResourceFactory(new ServiceContainer());

        var includeParser = new IncludeParser(options);
        var includeReader = new IncludeQueryStringParameterReader(includeParser, request, resourceGraph);

        var filterScopeParser = new QueryStringParameterScopeParser();
        var filterValueParser = new FilterParser(resourceFactory);
        var filterReader = new FilterQueryStringParameterReader(filterScopeParser, filterValueParser, request, resourceGraph, options);

        var sortScopeParser = new QueryStringParameterScopeParser();
        var sortValueParser = new SortParser();
        var sortReader = new SortQueryStringParameterReader(sortScopeParser, sortValueParser, request, resourceGraph);

        var sparseFieldSetScopeParser = new SparseFieldTypeParser(resourceGraph);
        var sparseFieldSetValueParser = new SparseFieldSetParser();
        var sparseFieldSetReader = new SparseFieldSetQueryStringParameterReader(sparseFieldSetScopeParser, sparseFieldSetValueParser, request, resourceGraph);

        var paginationParser = new PaginationParser();
        var paginationReader = new PaginationQueryStringParameterReader(paginationParser, request, resourceGraph, options);

        IQueryStringParameterReader[] readers =
        [
            includeReader,
            filterReader,
            sortReader,
            sparseFieldSetReader,
            paginationReader
        ];

        _queryStringReader = new QueryStringReader(options, _queryStringAccessor, readers, NullLoggerFactory.Instance);
    }

    [Benchmark]
    [ArgumentsSource(nameof(QueryStrings))]
    public void ReadAll(QueryStringArgument argument)
    {
        _queryStringAccessor.Query = argument.Query;
        _queryStringReader.ReadAll(null);
    }

    public IEnumerable<object> QueryStrings()
    {
        foreach (string queryString in new[]
        {
            "sort=alt-attr-name",
            "sort=-alt-attr-name",
            "filter=equals(alt-attr-name,'abc')&sort=-alt-attr-name&include=child&page[size]=1&fields[alt-resource-name]=alt-attr-name"
        })
        {
            yield return new QueryStringArgument(queryString);
        }
    }

    public sealed class QueryStringArgument(string queryString)
    {
        private readonly string _queryString = queryString;

        internal IQueryCollection Query { get; } = new QueryCollection(QueryHelpers.ParseQuery(queryString));

        public override string ToString()
        {
            return _queryString;
        }
    }
}
