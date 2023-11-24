using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Serialization.Response;

/// <inheritdoc cref="IETagGenerator" />
internal sealed class ETagGenerator : IETagGenerator
{
    private readonly IFingerprintGenerator _fingerprintGenerator;

    public ETagGenerator(IFingerprintGenerator fingerprintGenerator)
    {
        ArgumentGuard.NotNull(fingerprintGenerator);

        _fingerprintGenerator = fingerprintGenerator;
    }

    /// <inheritdoc />
    public EntityTagHeaderValue Generate(string requestUrl, string responseBody)
    {
        string[] elements =
        [
            requestUrl,
            responseBody
        ];

        string fingerprint = _fingerprintGenerator.Generate(elements);
        string eTagValue = $"\"{fingerprint}\"";

        return EntityTagHeaderValue.Parse(eTagValue);
    }
}
