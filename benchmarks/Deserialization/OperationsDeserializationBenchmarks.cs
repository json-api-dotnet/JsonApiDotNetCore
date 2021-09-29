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
    public class OperationsDeserializationBenchmarks : DeserializationBenchmarkBase
    {
        private static readonly string RequestBody = JsonSerializer.Serialize(new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "resourceAs",
                        lid = "a-1",
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
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "resourceAs",
                        id = "1",
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
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "resourceAs",
                        lid = "a-1"
                    }
                }
            }
        }).Replace("atomic__operations", "atomic:operations");

        [Benchmark]
        public object DeserializeOperationsRequest()
        {
            var document = JsonSerializer.Deserialize<Document>(RequestBody, SerializerReadOptions);
            return DocumentAdapter.Convert(document);
        }

        protected override JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph)
        {
            return new()
            {
                Kind = EndpointKind.AtomicOperations
            };
        }
    }
}
