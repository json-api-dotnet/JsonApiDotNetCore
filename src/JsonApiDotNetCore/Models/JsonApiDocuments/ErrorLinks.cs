using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    public sealed class ErrorLinks
    {
        /// <summary>
        /// A URL that leads to further details about this particular occurrence of the problem.
        /// </summary>
        [JsonProperty]
        public string About { get; set; }
    }
}
