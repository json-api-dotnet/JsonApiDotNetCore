using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;

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
        public void Current() => GetNameSpaceFromPath_Current(PATH, ENTITY_NAME);

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

        public static string GetNameSpaceFromPath_Current(string path, string entityName)
            => JsonApiDotNetCore.Builders.LinkBuilder.GetNamespaceFromPath(path, entityName);
    }
}
