using System.Net;
using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Primitives;
using JetBrains.Annotations;

namespace TestBuildingBlocks
{
    [PublicAPI]
    public static class HttpResponseMessageExtensions
    {
        public static HttpResponseMessageAssertions Should(this HttpResponseMessage instance)
        {
            return new(instance);
        }

        public sealed class HttpResponseMessageAssertions : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>
        {
            protected override string Identifier => "response";

            public HttpResponseMessageAssertions(HttpResponseMessage instance)
                : base(instance)
            {
            }

            // ReSharper disable once UnusedMethodReturnValue.Global
            [CustomAssertion]
            public AndConstraint<HttpResponseMessageAssertions> HaveStatusCode(HttpStatusCode statusCode)
            {
                if (Subject.StatusCode != statusCode)
                {
                    string responseText = Subject.Content.ReadAsStringAsync().Result;
                    Subject.StatusCode.Should().Be(statusCode, string.IsNullOrEmpty(responseText) ? null : $"response body returned was:\n{responseText}");
                }

                return new AndConstraint<HttpResponseMessageAssertions>(this);
            }
        }
    }
}
