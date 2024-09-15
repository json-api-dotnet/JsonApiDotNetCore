using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using Benchmarks.Tools;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.QueryString;

// ReSharper disable once ClassCanBeSealed.Global
[MarkdownExporter]
[SimpleJob(3, 10, 20)]
[MemoryDiagnoser]
public class QueryStringParserBenchmarks : IDisposable
{
    private readonly ServiceContainer _serviceProvider = new();
    private readonly FakeRequestQueryStringAccessor _queryStringAccessor = new();
    private readonly QueryStringReader _queryStringReader;

    public QueryStringParserBenchmarks()
    {
        IJsonApiOptions options = new JsonApiOptions
        {
            EnableLegacyFilterNotation = true
        };

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<QueryableResource, int>("alt-resource-name").Build();

        var request = new JsonApiRequest
        {
            PrimaryResourceType = resourceGraph.GetResourceType(typeof(QueryableResource)),
            IsCollection = true
        };

        var resourceFactory = new ResourceFactory(_serviceProvider);

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
        const string queryString = "?filter[alt-attr-name]=abc,eq:abc&sort=-alt-attr-name&include=child&page[size]=1&fields[alt-resource-name]=alt-attr-name";

        _queryStringAccessor.SetQueryString(queryString);
        _queryStringReader.ReadAll(null);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

#pragma warning disable CA1063 // Implement IDisposable Correctly
    private void Dispose(bool disposing)
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        if (disposing)
        {
            _serviceProvider.Dispose();
        }
    }
}
