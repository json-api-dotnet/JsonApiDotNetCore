using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using JsonApiDotNetCore.Extensions;

namespace Benchmarks.LinkBuilder
{
    [MarkdownExporter, SimpleJob(launchCount : 3, warmupCount : 10, targetCount : 20), MemoryDiagnoser]
    public class LinkBuilder_GetNamespaceFromPath_Benchmarks
    {
        private const string PATH = "/api/some-really-long-namespace-path/resources/current/articles";
        private const string ENTITY_NAME = "articles";

        [Benchmark]
        public void UsingSplit() => GetNamespaceFromPath_BySplitting(PATH, ENTITY_NAME);

        [Benchmark]
        public void UsingSpanWithStringBuilder() => GetNamespaceFromPath_Using_Span_With_StringBuilder(PATH, ENTITY_NAME);

        [Benchmark]
        public void UsingSpanWithNoAlloc() => GetNamespaceFromPath_Using_Span_No_Alloc(PATH, ENTITY_NAME);

        public static string GetNamespaceFromPath_BySplitting(string path, string entityName)
        {
            var nSpace = string.Empty;
            var segments = path.Split('/');

            for (var i = 1; i < segments.Length; i++)
            {
                if (segments[i].ToLower() == entityName)
                    break;

                nSpace += $"/{segments[i]}";
            }

            return nSpace;
        }

        public static string GetNamespaceFromPath_Using_Span_No_Alloc(string path, string entityName)
        {
            var entityNameSpan = entityName.AsSpan();
            var pathSpan = path.AsSpan();
            const char delimiter = '/';
            for (var i = 0; i < pathSpan.Length; i++)
            {
                if(pathSpan[i].Equals(delimiter))
                {
                    var nextPosition = i+1;
                    if(pathSpan.Length > i + entityNameSpan.Length)
                    {
                        var possiblePathSegment = pathSpan.Slice(nextPosition, entityNameSpan.Length);
                        if (entityNameSpan.SequenceEqual(possiblePathSegment)) 
                        {
                            // check to see if it's the last position in the string
                            //   or if the next character is a /
                            var lastCharacterPosition = nextPosition + entityNameSpan.Length;

                            if(lastCharacterPosition == pathSpan.Length || pathSpan.Length >= lastCharacterPosition + 2 && pathSpan[lastCharacterPosition + 1].Equals(delimiter))
                            {
                                return pathSpan.Slice(0, i).ToString();
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static string GetNamespaceFromPath_Using_Span_With_StringBuilder(string path, string entityName)
        {
            var sb = new StringBuilder();
            var entityNameSpan = entityName.AsSpan();
            var subSpans = path.SpanSplit('/');
            for (var i = 1; i < subSpans.Count; i++)
            {
                var span = subSpans[i];
                if (entityNameSpan.SequenceEqual(span)) 
                    break;

                sb.Append($"/{span.ToString()}");
            }
            return sb.ToString();
        }
    }
}
