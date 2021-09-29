using System;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;

namespace Benchmarks.Deserialization
{
    [MarkdownExporter]
    // ReSharper disable once ClassCanBeSealed.Global
    public class ResourceDeserializationBenchmarks : DeserializationBenchmarkBase
    {
        private static readonly string RequestBody = JsonSerializer.Serialize(new
        {
            data = new
            {
                type = "resourceAs",
                attributes = new
                {
                    attribute01 = true,
                    attribute02 = 'A',
                    attribute03 = 100UL,
                    attribute04 = 100.001m,
                    attribute05 = 200.002f,
                    attribute06 = "text",
                    attribute07 = DateTime.MaxValue,
                    attribute08 = DateTimeOffset.MaxValue,
                    attribute09 = TimeSpan.MaxValue,
                    attribute10 = DayOfWeek.Friday
                },
                relationships = new
                {
                    single1 = new
                    {
                        data = new
                        {
                            type = "resourceAs",
                            id = "101"
                        }
                    },
                    single2 = new
                    {
                        data = new
                        {
                            type = "resourceAs",
                            id = "102"
                        }
                    },
                    single3 = new
                    {
                        data = new
                        {
                            type = "resourceAs",
                            id = "103"
                        }
                    },
                    single4 = new
                    {
                        data = new
                        {
                            type = "resourceAs",
                            id = "104"
                        }
                    },
                    single5 = new
                    {
                        data = new
                        {
                            type = "resourceAs",
                            id = "105"
                        }
                    },
                    multi1 = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "resourceAs",
                                id = "201"
                            }
                        }
                    },
                    multi2 = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "resourceAs",
                                id = "202"
                            }
                        }
                    },
                    multi3 = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "resourceAs",
                                id = "203"
                            }
                        }
                    },
                    multi4 = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "resourceAs",
                                id = "204"
                            }
                        }
                    },
                    multi5 = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "resourceAs",
                                id = "205"
                            }
                        }
                    }
                }
            }
        });

        [Benchmark]
        public object DeserializeResourceRequest()
        {
            var document = JsonSerializer.Deserialize<Document>(RequestBody, SerializerReadOptions);

            return DocumentAdapter.Convert(document);
        }

        protected override JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph)
        {
            return new()
            {
                Kind = EndpointKind.Primary,
                PrimaryResource = resourceGraph.GetResourceContext<ResourceA>(),
                WriteOperation = WriteOperationKind.CreateResource
            };
        }
    }
}
