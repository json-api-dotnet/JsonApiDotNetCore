using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace Benchmarks.Serialization
{
    [MarkdownExporter]
    // ReSharper disable once ClassCanBeSealed.Global
    public class OperationsSerializationBenchmarks : SerializationBenchmarkBase
    {
        private readonly IEnumerable<OperationContainer> _responseOperations;

        public OperationsSerializationBenchmarks()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            JsonApiRequest request = CreateJsonApiRequest(ResourceGraph);

            _responseOperations = CreateResponseOperations(request);
        }

        private static IEnumerable<OperationContainer> CreateResponseOperations(IJsonApiRequest request)
        {
            var resource1 = new OutgoingResource
            {
                Id = 1,
                Attribute01 = true,
                Attribute02 = 'A',
                Attribute03 = 100UL,
                Attribute04 = 100.001m,
                Attribute05 = 100.002f,
                Attribute06 = "text1",
                Attribute07 = new DateTime(2001, 1, 1),
                Attribute08 = new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.FromHours(1)),
                Attribute09 = new TimeSpan(1, 0, 0),
                Attribute10 = DayOfWeek.Sunday
            };

            var resource2 = new OutgoingResource
            {
                Id = 2,
                Attribute01 = false,
                Attribute02 = 'B',
                Attribute03 = 200UL,
                Attribute04 = 200.001m,
                Attribute05 = 200.002f,
                Attribute06 = "text2",
                Attribute07 = new DateTime(2002, 2, 2),
                Attribute08 = new DateTimeOffset(2002, 2, 2, 0, 0, 0, TimeSpan.FromHours(2)),
                Attribute09 = new TimeSpan(2, 0, 0),
                Attribute10 = DayOfWeek.Monday
            };

            var resource3 = new OutgoingResource
            {
                Id = 3,
                Attribute01 = true,
                Attribute02 = 'C',
                Attribute03 = 300UL,
                Attribute04 = 300.001m,
                Attribute05 = 300.002f,
                Attribute06 = "text3",
                Attribute07 = new DateTime(2003, 3, 3),
                Attribute08 = new DateTimeOffset(2003, 3, 3, 0, 0, 0, TimeSpan.FromHours(3)),
                Attribute09 = new TimeSpan(3, 0, 0),
                Attribute10 = DayOfWeek.Tuesday
            };

            var resource4 = new OutgoingResource
            {
                Id = 4,
                Attribute01 = false,
                Attribute02 = 'D',
                Attribute03 = 400UL,
                Attribute04 = 400.001m,
                Attribute05 = 400.002f,
                Attribute06 = "text4",
                Attribute07 = new DateTime(2004, 4, 4),
                Attribute08 = new DateTimeOffset(2004, 4, 4, 0, 0, 0, TimeSpan.FromHours(4)),
                Attribute09 = new TimeSpan(4, 0, 0),
                Attribute10 = DayOfWeek.Wednesday
            };

            var resource5 = new OutgoingResource
            {
                Id = 5,
                Attribute01 = true,
                Attribute02 = 'E',
                Attribute03 = 500UL,
                Attribute04 = 500.001m,
                Attribute05 = 500.002f,
                Attribute06 = "text5",
                Attribute07 = new DateTime(2005, 5, 5),
                Attribute08 = new DateTimeOffset(2005, 5, 5, 0, 0, 0, TimeSpan.FromHours(5)),
                Attribute09 = new TimeSpan(5, 0, 0),
                Attribute10 = DayOfWeek.Thursday
            };

            var targetedFields = new TargetedFields();

            return new List<OperationContainer>
            {
                new(resource1, targetedFields, request),
                new(resource2, targetedFields, request),
                new(resource3, targetedFields, request),
                new(resource4, targetedFields, request),
                new(resource5, targetedFields, request)
            };
        }

        [Benchmark]
        public string SerializeOperationsResponse()
        {
            Document responseDocument = ResponseModelAdapter.Convert(_responseOperations);
            return JsonSerializer.Serialize(responseDocument, SerializerWriteOptions);
        }

        protected override JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph)
        {
            return new JsonApiRequest
            {
                Kind = EndpointKind.AtomicOperations,
                PrimaryResourceType = resourceGraph.GetResourceType<OutgoingResource>()
            };
        }

        protected override IEvaluatedIncludeCache CreateEvaluatedIncludeCache(IResourceGraph resourceGraph)
        {
            return new EvaluatedIncludeCache();
        }
    }
}
