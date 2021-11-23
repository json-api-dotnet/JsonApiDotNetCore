using System.Collections.Generic;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Response
{
    /// <summary>
    /// Provides a method to generate a fingerprint for a collection of string values.
    /// </summary>
    [PublicAPI]
    public interface IFingerprintGenerator
    {
        /// <summary>
        /// Generates a fingerprint for the specified elements.
        /// </summary>
        public string Generate(IEnumerable<string> elements);
    }
}
