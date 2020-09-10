using System;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmarks.LinkBuilder
{
    [MarkdownExporter, SimpleJob(launchCount: 3, warmupCount: 10, targetCount: 20), MemoryDiagnoser]
    public class LinkBuilderGetNamespaceFromPathBenchmarks
    {
        private const string RequestPath = "/api/some-really-long-namespace-path/resources/current/articles/?some";
        private const string ResourceName = "articles";
        private const char PathDelimiter = '/';

        [Benchmark]
        public void UsingStringSplit() => GetNamespaceFromPathUsingStringSplit(RequestPath, ResourceName);

        [Benchmark]
        public void UsingReadOnlySpan() => GetNamespaceFromPathUsingReadOnlySpan(RequestPath, ResourceName);

        public static string GetNamespaceFromPathUsingStringSplit(string path, string resourceName)
        {
            StringBuilder namespaceBuilder = new StringBuilder(path.Length);
            string[] segments = path.Split('/');

            for (int index = 1; index < segments.Length; index++)
            {
                if (segments[index] == resourceName)
                {
                    break;
                }

                namespaceBuilder.Append(PathDelimiter);
                namespaceBuilder.Append(segments[index]);
            }

            return namespaceBuilder.ToString();
        }

        public static string GetNamespaceFromPathUsingReadOnlySpan(string path, string resourceName)
        {
            ReadOnlySpan<char> resourceNameSpan = resourceName.AsSpan();
            ReadOnlySpan<char> pathSpan = path.AsSpan();

            for (int index = 0; index < pathSpan.Length; index++)
            {
                if (pathSpan[index].Equals(PathDelimiter))
                {
                    if (pathSpan.Length > index + resourceNameSpan.Length)
                    {
                        ReadOnlySpan<char> possiblePathSegment = pathSpan.Slice(index + 1, resourceNameSpan.Length);

                        if (resourceNameSpan.SequenceEqual(possiblePathSegment))
                        {
                            int lastCharacterIndex = index + 1 + resourceNameSpan.Length;

                            bool isAtEnd = lastCharacterIndex == pathSpan.Length;
                            bool hasDelimiterAfterSegment = pathSpan.Length >= lastCharacterIndex + 1 && pathSpan[lastCharacterIndex].Equals(PathDelimiter);
                            if (isAtEnd || hasDelimiterAfterSegment)
                            {
                                return pathSpan.Slice(0, index).ToString();
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
