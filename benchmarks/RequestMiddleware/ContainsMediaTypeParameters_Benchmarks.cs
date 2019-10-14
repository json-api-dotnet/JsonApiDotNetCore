using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using JsonApiDotNetCore.Internal;

namespace Benchmarks.RequestMiddleware
{
    [MarkdownExporter, MemoryDiagnoser]
    public class ContainsMediaTypeParameters_Benchmarks
    {
        private const string MEDIA_TYPE = "application/vnd.api+json; version=1";

        [Benchmark]
        public void UsingSplit() => UsingSplitImpl(MEDIA_TYPE);

        [Benchmark]
        public void Current() 
            => JsonApiDotNetCore.Middleware.CurrentRequestMiddleware.ContainsMediaTypeParameters(MEDIA_TYPE);

        private bool UsingSplitImpl(string mediaType)
        {
            var mediaTypeArr = mediaType.Split(';');
            return (mediaTypeArr[0] ==  Constants.ContentType && mediaTypeArr.Length == 2);	
        }
    }
}
