using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    internal sealed class ETagGenerator : IETagGenerator
    {
        private readonly IFingerprintGenerator _fingerprintGenerator;

        public ETagGenerator(IFingerprintGenerator fingerprintGenerator)
        {
            ArgumentGuard.NotNull(fingerprintGenerator, nameof(fingerprintGenerator));

            _fingerprintGenerator = fingerprintGenerator;
        }

        /// <inheritdoc />
        public EntityTagHeaderValue Generate(string requestUrl, string responseBody)
        {
            string fingerprint = _fingerprintGenerator.Generate(ArrayFactory.Create(requestUrl, responseBody));
            string eTagValue = $"\"{fingerprint}\"";

            return EntityTagHeaderValue.Parse(eTagValue);
        }
    }
}
