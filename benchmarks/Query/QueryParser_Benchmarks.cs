using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Benchmarks.Query {
    [MarkdownExporter, SimpleJob(launchCount : 3, warmupCount : 10, targetCount : 20), MemoryDiagnoser]
    public class QueryParser_Benchmarks {
        private readonly BenchmarkFacade _queryParser;

        private const string ATTRIBUTE = "Attribute";
        private const string ASCENDING_SORT = ATTRIBUTE;
        private const string DESCENDING_SORT = "-" + ATTRIBUTE;

        public QueryParser_Benchmarks() {
            var requestMock = new Mock<IRequestManager>();
            requestMock.Setup(m => m.GetRequestResource()).Returns(new ContextEntity {
                Attributes = new List<AttrAttribute> {
                    new AttrAttribute(ATTRIBUTE, ATTRIBUTE)
                }
            });
            var options = new JsonApiOptions();
            _queryParser = new BenchmarkFacade(requestMock.Object, options);
        }

        [Benchmark]
        public void AscendingSort() => _queryParser._ParseSortParameters(ASCENDING_SORT);

        [Benchmark]
        public void DescendingSort() => _queryParser._ParseSortParameters(DESCENDING_SORT);

        [Benchmark]
        public void ComplexQuery() => Run(100, () => _queryParser.Parse(
            new QueryCollection(
                new Dictionary<string, StringValues> { 
                    { $"filter[{ATTRIBUTE}]", new StringValues(new [] { "abc", "eq:abc" }) },
                    { $"sort", $"-{ATTRIBUTE}" },
                    { $"include", "relationship" },
                    { $"page[size]", "1" },
                    { $"fields[resource]", ATTRIBUTE },
                }
            )
        ));

        private void Run(int iterations, Action action) { 
            for (int i = 0; i < iterations; i++)
                action();
        }

        // this facade allows us to expose and micro-benchmark protected methods
        private class BenchmarkFacade : QueryParser {
            public BenchmarkFacade(
                IRequestManager requestManager,
                JsonApiOptions options) : base(requestManager, options) { }

            public void _ParseSortParameters(string value) => base.ParseSortParameters(value);
        }
    }
}
