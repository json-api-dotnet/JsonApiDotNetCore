using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using System;

namespace Benchmarks.LinkBuilder
{
    [MarkdownExporter, SimpleJob(launchCount: 3, warmupCount: 10, targetCount: 20), MemoryDiagnoser]
    public class LinkBuilder_GetNamespaceFromPath_Benchmarks
    {
        private const string PATH = "/api/some-really-long-namespace-path/resources/current/articles";
        private const string ENTITY_NAME = "articles";

        [Benchmark]
        public void UsingSplit() => GetNamespaceFromPath_BySplitting(PATH, ENTITY_NAME);

        [Benchmark]
        public void Current() => GetNameSpaceFromPathCurrent(PATH, ENTITY_NAME);

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

        public static string GetNameSpaceFromPathCurrent(string path, string entityName)
        {

            var entityNameSpan = entityName.AsSpan();
            var pathSpan = path.AsSpan();
            const char delimiter = '/';
            for (var i = 0; i < pathSpan.Length; i++)
            {
                if (pathSpan[i].Equals(delimiter))
                {
                    var nextPosition = i + 1;
                    if (pathSpan.Length > i + entityNameSpan.Length)
                    {
                        var possiblePathSegment = pathSpan.Slice(nextPosition, entityNameSpan.Length);
                        if (entityNameSpan.SequenceEqual(possiblePathSegment))
                        {
                            // check to see if it's the last position in the string
                            //   or if the next character is a /
                            var lastCharacterPosition = nextPosition + entityNameSpan.Length;

                            if (lastCharacterPosition == pathSpan.Length || pathSpan.Length >= lastCharacterPosition + 2 && pathSpan[lastCharacterPosition].Equals(delimiter))
                            {
                                return pathSpan.Slice(0, i).ToString();
                            }
                        }
                    }
                }
            }

            return string.Empty;


        }
    }
}
